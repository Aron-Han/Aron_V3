using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Aron_V3
{
	public static class StepDisplayBindingRunner
	{
		public static void TryPublishStepImage(
			string jobName,
			string taskName,
			StepConfig step,
			StepResult stepRunResult,
			VisionRunContext context)
		{
			if (step == null)
			{
				return;
			}

			if (string.IsNullOrWhiteSpace(step.DisplaySlotName) ||
				string.Equals(step.DisplaySlotName, "Not Show", StringComparison.OrdinalIgnoreCase))
			{
				return;
			}

			VisionImage selectedImage = TryGetOutputVisionImage(stepRunResult, step.DisplayOutputKey);
			object rawImage = selectedImage == null ? null : selectedImage.RawImage;
			Bitmap bitmap = TryGetPreparedDisplayBitmap(selectedImage);

			if (bitmap == null && rawImage == null && step.StepType != StepType.Halcon)
			{
				rawImage = TryGetFirstInputImage(step, context);
				selectedImage = null;
			}

			if (bitmap == null)
			{
				bitmap = ImageConvertHelper.TryConvertToBitmap(rawImage);
			}

			if (bitmap == null)
			{
				LogDisplayPublishFailure(
					"Display image conversion failed",
					jobName,
					taskName,
					step,
					stepRunResult,
					selectedImage,
					rawImage);
				return;
			}

			if (step.StepType == StepType.Halcon && IsNearlyBlackBitmap(bitmap))
			{
				LogDisplayPublishFailure(
					"Hdev display bitmap appears blank or black",
					jobName,
					taskName,
					step,
					stepRunResult,
					selectedImage,
					rawImage);
			}

			try
			{
				string sourceInfo = jobName + " / " + taskName + " / " + step.StepName;
				bool inspectionOK = ResolveInspectionResult(step, stepRunResult);

				DisplayRuntimeManager.ShowImage(
					step.DisplaySlotName,
					bitmap,
					sourceInfo,
					step.DisplayMode,
					selectedImage == null ? null : selectedImage.DisplayRecord,
					selectedImage == null ? string.Empty : selectedImage.DisplayRecordKey,
					inspectionOK,
					jobName,
					ResolveContextValue(context, "JobID", "JobID0", "JobID_0", "Comm.JobID0", "TCP/IP.JobID0"),
					ResolveContextValue(context, "PosID", "PosID0", "PosID_0", "Comm.PosID0", "TCP/IP.PosID0"),
					ResolveContextValue(context, "Comm.Channel", "Task.CommunicationChannel", "Channel", "EngineID", "EngineID", "Engine", "Comm.EngineID"));
			}
			finally
			{
				bitmap.Dispose();
			}
		}

		private static bool IsNearlyBlackBitmap(Bitmap bitmap)
		{
			if (bitmap == null || bitmap.Width <= 0 || bitmap.Height <= 0)
			{
				return false;
			}

			try
			{
				int samples = 0;
				int brightSamples = 0;
				int stepX = Math.Max(1, bitmap.Width / 24);
				int stepY = Math.Max(1, bitmap.Height / 24);

				for (int y = 0; y < bitmap.Height; y += stepY)
				{
					for (int x = 0; x < bitmap.Width; x += stepX)
					{
						Color color = bitmap.GetPixel(x, y);
						int brightness = Math.Max(color.R, Math.Max(color.G, color.B));
						if (brightness > 12)
						{
							brightSamples++;
						}

						samples++;
					}
				}

				return samples > 0 && brightSamples <= Math.Max(1, samples / 100);
			}
			catch
			{
				return false;
			}
		}

		private static Bitmap TryGetPreparedDisplayBitmap(VisionImage image)
		{
			if (image == null || image.DisplayBitmap == null)
			{
				return null;
			}

			try
			{
				return new Bitmap(image.DisplayBitmap);
			}
			catch
			{
				return null;
			}
		}

		private static VisionImage TryGetOutputVisionImage(StepResult result, string outputKey)
		{
			if (result == null || string.IsNullOrWhiteSpace(outputKey))
			{
				return null;
			}

			VisionImage image;
			return result.OutputImages.TryGetValue(outputKey, out image) ? image : null;
		}

		private static object TryGetFirstInputImage(StepConfig step, VisionRunContext context)
		{
			if (step == null || context == null)
			{
				return null;
			}

			foreach (string key in RuntimeImageSourceParser.SplitImageSources(step.InputImageKey))
			{
				VisionImage image;
				if (context.TryGetImage(key, out image) && image != null && image.RawImage != null)
				{
					return image.RawImage;
				}
			}

			return null;
		}

		private static void LogDisplayPublishFailure(
			string reason,
			string jobName,
			string taskName,
			StepConfig step,
			StepResult result,
			VisionImage selectedImage,
			object rawImage)
		{
			try
			{
				string availableImages = string.Empty;
				if (result != null && result.OutputImages != null && result.OutputImages.Count > 0)
				{
					availableImages = string.Join(",", result.OutputImages.Keys.ToArray());
				}

				RuntimeLogStore.Append(
					DateTime.Now,
					RuntimeLogCategory.Step,
					reason +
					". Job=" + (jobName ?? string.Empty) +
					", Task=" + (taskName ?? string.Empty) +
					", Step=" + (step == null ? string.Empty : step.StepName) +
					", Slot=" + (step == null ? string.Empty : step.DisplaySlotName) +
					", OutputKey=" + (step == null ? string.Empty : step.DisplayOutputKey) +
					", AvailableImages=" + availableImages +
					", VisionImageType=" + (selectedImage == null ? "null" : (selectedImage.ImageType ?? string.Empty)) +
					", RawType=" + GetDebugTypeName(rawImage),
					true);
			}
			catch
			{
			}
		}

		private static string GetDebugTypeName(object value)
		{
			return value == null ? "null" : (value.GetType().FullName ?? value.GetType().Name);
		}

		private static object TryGetOutputImage(object runResult, string outputKey)
		{
			if (runResult == null || string.IsNullOrWhiteSpace(outputKey))
			{
				return null;
			}

			if (runResult is Bitmap)
			{
				return runResult;
			}

			StepResult stepResult = runResult as StepResult;
			if (stepResult != null)
			{
				VisionImage image;
				if (!string.IsNullOrWhiteSpace(outputKey) &&
					stepResult.OutputImages.TryGetValue(outputKey, out image) &&
					image != null)
				{
					return image.RawImage;
				}

			}

			Type type = runResult.GetType();

			PropertyInfo property = type.GetProperty(outputKey, BindingFlags.Instance | BindingFlags.Public);

			if (property != null)
			{
				return property.GetValue(runResult, null);
			}

			FieldInfo field = type.GetField(outputKey, BindingFlags.Instance | BindingFlags.Public);

			if (field != null)
			{
				return field.GetValue(runResult);
			}

			PropertyInfo outputsProp = type.GetProperty("Outputs", BindingFlags.Instance | BindingFlags.Public);

			if (outputsProp != null)
			{
				object outputs = outputsProp.GetValue(runResult, null);
				object value = TryReadNamedOutput(outputs, outputKey);

				if (value != null)
				{
					return value;
				}
			}

			if (outputKey.IndexOf(".", StringComparison.OrdinalIgnoreCase) > 0)
			{
				string[] parts = outputKey.Split(new char[] { '.' }, 2);

				object parent = TryGetOutputImage(runResult, parts[0]);

				if (parent != null)
				{
					return TryGetOutputImage(parent, parts[1]);
				}
			}

			return null;
		}

		private static object TryReadNamedOutput(object outputs, string outputKey)
		{
			if (outputs == null)
			{
				return null;
			}

			try
			{
				object item = null;

				PropertyInfo itemProp = outputs.GetType().GetProperty("Item", new Type[] { typeof(string) });

				if (itemProp != null)
				{
					item = itemProp.GetValue(outputs, new object[] { outputKey });
				}

				if (item == null)
				{
					MethodInfo getItem = outputs.GetType().GetMethod("get_Item", new Type[] { typeof(string) });

					if (getItem != null)
					{
						item = getItem.Invoke(outputs, new object[] { outputKey });
					}
				}

				if (item == null)
				{
					return null;
				}

				PropertyInfo valueProp = item.GetType().GetProperty("Value", BindingFlags.Instance | BindingFlags.Public);

				if (valueProp != null)
				{
					return valueProp.GetValue(item, null);
				}

				return item;
			}
			catch
			{
				return null;
			}
		}

		private static bool ResolveInspectionResult(StepConfig step, StepResult result)
		{
			if (step == null ||
				string.IsNullOrWhiteSpace(step.DisplayResultKey) ||
				string.Equals(step.DisplayResultKey, "Not Use", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}

			object value;
			if (result != null &&
				result.Outputs != null &&
				TryGetOutputValue(result.Outputs, step.DisplayResultKey, out value))
			{
				return ConvertToBool(value);
			}

			return false;
		}

		private static bool TryGetOutputValue(Dictionary<string, object> values, string key, out object value)
		{
			value = null;

			if (values == null || string.IsNullOrWhiteSpace(key))
			{
				return false;
			}

			if (values.TryGetValue(key, out value))
			{
				return true;
			}

			foreach (KeyValuePair<string, object> pair in values)
			{
				if (!string.IsNullOrEmpty(pair.Key) &&
					pair.Key.EndsWith("." + key, StringComparison.OrdinalIgnoreCase))
				{
					value = pair.Value;
					return true;
				}
			}

			return false;
		}

		private static bool ConvertToBool(object value)
		{
			if (value == null)
			{
				return false;
			}

			bool boolValue;
			if (bool.TryParse(Convert.ToString(value), out boolValue))
			{
				return boolValue;
			}

			double numericValue;
			if (double.TryParse(Convert.ToString(value), out numericValue))
			{
				return Math.Abs(numericValue) > 0.000001;
			}

			string text = Convert.ToString(value);
			return string.Equals(text, "OK", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(text, "PASS", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(text, "TRUE", StringComparison.OrdinalIgnoreCase);
		}

		private static string ResolveContextValue(VisionRunContext context, params string[] keys)
		{
			if (context == null || keys == null)
			{
				return string.Empty;
			}

			foreach (string key in keys)
			{
				object value;
				if (!string.IsNullOrWhiteSpace(key) &&
					context.TryGetData(key, out value) &&
					value != null)
				{
					return Convert.ToString(value);
				}
			}

			return string.Empty;
		}
	}

	public static class ImageConvertHelper
	{
		public static Bitmap TryConvertToBitmap(object image)
		{
			if (image == null)
			{
				return null;
			}

			Bitmap bmp = image as Bitmap;

			if (bmp != null)
			{
				return new Bitmap(bmp);
			}

			Bitmap visionProBitmap = VisionProImageBitmapConverter.TryConvertToBitmap(image);
			if (visionProBitmap != null)
			{
				return visionProBitmap;
			}

			try
			{
				MethodInfo toBitmapMethod = image.GetType().GetMethod("ToBitmap", Type.EmptyTypes);

				if (toBitmapMethod != null)
				{
					object result = toBitmapMethod.Invoke(image, null);
					Bitmap resultBmp = result as Bitmap;

					if (resultBmp != null)
					{
						return new Bitmap(resultBmp);
					}
				}
			}
			catch
			{
			}

			try
			{
				PropertyInfo bitmapProp = image.GetType().GetProperty("Bitmap", BindingFlags.Instance | BindingFlags.Public);

				if (bitmapProp != null)
				{
					object result = bitmapProp.GetValue(image, null);
					Bitmap resultBmp = result as Bitmap;

					if (resultBmp != null)
					{
						return new Bitmap(resultBmp);
					}
				}
			}
			catch
			{
			}

			Bitmap halconBitmap = TryConvertHalconImageToBitmap(image);
			if (halconBitmap != null)
			{
				return halconBitmap;
			}

			return null;
		}

		private static Bitmap TryConvertHalconImageToBitmap(object image)
		{
			object imageObject = image;
			bool disposeImageObject = false;

			try
			{
				Type imageType = imageObject.GetType();
				if ((imageType.FullName ?? string.Empty).IndexOf("HObject", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					Type hImageType = FindType("HalconDotNet.HImage");
					if (hImageType != null)
					{
						imageObject = Activator.CreateInstance(hImageType, new object[] { imageObject });
						disposeImageObject = true;
					}
				}

				Bitmap colorBitmap = TryConvertHalconColorImageToBitmap(imageObject);
				if (colorBitmap != null)
				{
					return colorBitmap;
				}

				Bitmap grayBitmap = TryConvertHalconGrayImageToBitmap(imageObject);
				if (grayBitmap != null)
				{
					return grayBitmap;
				}

				return TryConvertHalconImageViaTempFile(imageObject);
			}
			catch
			{
				return null;
			}
			finally
			{
				if (disposeImageObject)
				{
					IDisposable disposable = imageObject as IDisposable;
					if (disposable != null)
					{
						disposable.Dispose();
					}
				}
			}
		}

		private static Bitmap TryConvertHalconGrayImageToBitmap(object imageObject)
		{
			try
			{
				MethodInfo pointerMethod = FindImagePointerMethod(imageObject.GetType(), "GetImagePointer1", 3, typeof(IntPtr));
				if (pointerMethod == null)
				{
					return null;
				}

				object[] args = new object[] { null, null, null };
				object pointerValue = pointerMethod.Invoke(imageObject, args);
				if (pointerValue == null)
				{
					return null;
				}

				IntPtr pointer = (IntPtr)pointerValue;
				string halconType = Convert.ToString(args[0]);
				int width = Convert.ToInt32(args[1]);
				int height = Convert.ToInt32(args[2]);
				if (pointer == IntPtr.Zero || width <= 0 || height <= 0)
				{
					return null;
				}

				byte[] gray = CopyHalconGrayToByteBuffer(pointer, width, height, halconType);
				if (gray == null)
				{
					return null;
				}

				return CreateRgbBitmapFromGray(gray, width, height);
			}
			catch
			{
				return null;
			}
		}

		private static Bitmap TryConvertHalconImageViaTempFile(object imageObject)
		{
			string tempFile = null;

			try
			{
				MethodInfo writeImageMethod = FindWriteImageMethod(imageObject == null ? null : imageObject.GetType());
				if (writeImageMethod == null)
				{
					return null;
				}

				tempFile = Path.Combine(Path.GetTempPath(), "Aron_V3_Halcon_" + Guid.NewGuid().ToString("N") + ".bmp");
				writeImageMethod.Invoke(imageObject, CreateWriteImageArguments(writeImageMethod, tempFile));

				using (Bitmap fileBitmap = new Bitmap(tempFile))
				{
					return new Bitmap(fileBitmap);
				}
			}
			catch
			{
				return null;
			}
			finally
			{
				if (!string.IsNullOrWhiteSpace(tempFile))
				{
					try
					{
						if (File.Exists(tempFile))
						{
							File.Delete(tempFile);
						}
					}
					catch
					{
					}
				}
			}
		}

		private static Bitmap TryConvertHalconColorImageToBitmap(object imageObject)
		{
			try
			{
				MethodInfo pointerMethod = FindImagePointer3Method(imageObject.GetType());
				if (pointerMethod == null)
				{
					return null;
				}

				IntPtr[] pointers;
				string halconType;
				int width;
				int height;
				if (!TryInvokeGetImagePointer3(pointerMethod, imageObject, out pointers, out halconType, out width, out height))
				{
					return null;
				}

				if (width <= 0 || height <= 0 ||
					pointers[0] == IntPtr.Zero ||
					pointers[1] == IntPtr.Zero ||
					pointers[2] == IntPtr.Zero)
				{
					return null;
				}

				byte[] red = CopyHalconGrayToByteBuffer(pointers[0], width, height, halconType);
				byte[] green = CopyHalconGrayToByteBuffer(pointers[1], width, height, halconType);
				byte[] blue = CopyHalconGrayToByteBuffer(pointers[2], width, height, halconType);
				if (red == null || green == null || blue == null)
				{
					return null;
				}

				return CreateRgbBitmapFromChannels(red, green, blue, width, height);
			}
			catch
			{
				return null;
			}
		}

		private static bool TryInvokeGetImagePointer3(
			MethodInfo pointerMethod,
			object imageObject,
			out IntPtr[] pointers,
			out string halconType,
			out int width,
			out int height)
		{
			pointers = null;
			halconType = string.Empty;
			width = 0;
			height = 0;

			ParameterInfo[] parameters = pointerMethod.GetParameters();
			if (parameters.Length == 6)
			{
				object[] args = new object[] { null, null, null, null, null, null };
				pointerMethod.Invoke(imageObject, args);
				pointers = new IntPtr[]
				{
					ToIntPtr(args[0]),
					ToIntPtr(args[1]),
					ToIntPtr(args[2])
				};
				halconType = Convert.ToString(args[3]);
				width = Convert.ToInt32(args[4]);
				height = Convert.ToInt32(args[5]);
				return true;
			}

			if (parameters.Length == 3)
			{
				object[] args = new object[] { null, null, null };
				object pointerValue = pointerMethod.Invoke(imageObject, args);
				pointers = ExtractHalconChannelPointers(pointerValue);
				halconType = Convert.ToString(args[0]);
				width = Convert.ToInt32(args[1]);
				height = Convert.ToInt32(args[2]);
				return pointers != null && pointers.Length >= 3;
			}

			return false;
		}

		private static IntPtr[] ExtractHalconChannelPointers(object pointerValue)
		{
			IntPtr[] direct = pointerValue as IntPtr[];
			if (direct != null)
			{
				return direct;
			}

			Array array = pointerValue as Array;
			if (array != null && array.Length >= 3)
			{
				IntPtr[] pointers = new IntPtr[3];
				for (int i = 0; i < 3; i++)
				{
					pointers[i] = ToIntPtr(array.GetValue(i));
				}

				return pointers;
			}

			return null;
		}

		private static IntPtr ToIntPtr(object value)
		{
			if (value == null)
			{
				return IntPtr.Zero;
			}

			if (value is IntPtr)
			{
				return (IntPtr)value;
			}

			return new IntPtr(Convert.ToInt64(value));
		}

		private static bool ResolveInspectionResult(StepConfig step, StepResult result)
		{
			if (step == null ||
				string.IsNullOrWhiteSpace(step.DisplayResultKey) ||
				step.DisplayResultKey.Equals("Not Use", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}

			object value;
			if (result != null &&
				result.Outputs != null &&
				TryGetOutputValue(result.Outputs, step.DisplayResultKey, out value))
			{
				return ConvertToBool(value);
			}

			return false;
		}

		private static bool TryGetOutputValue(System.Collections.Generic.Dictionary<string, object> values, string key, out object value)
		{
			value = null;
			if (values == null || string.IsNullOrWhiteSpace(key))
			{
				return false;
			}

			if (values.TryGetValue(key, out value))
			{
				return true;
			}

			foreach (System.Collections.Generic.KeyValuePair<string, object> pair in values)
			{
				if (pair.Key.EndsWith("." + key, StringComparison.OrdinalIgnoreCase))
				{
					value = pair.Value;
					return true;
				}
			}

			return false;
		}

		private static bool ConvertToBool(object value)
		{
			if (value == null)
			{
				return false;
			}

			bool boolValue;
			if (bool.TryParse(Convert.ToString(value), out boolValue))
			{
				return boolValue;
			}

			double number;
			if (double.TryParse(Convert.ToString(value), out number))
			{
				return Math.Abs(number) > 0.000001;
			}

			string text = Convert.ToString(value);
			return text.Equals("OK", StringComparison.OrdinalIgnoreCase) ||
				text.Equals("PASS", StringComparison.OrdinalIgnoreCase) ||
				text.Equals("TRUE", StringComparison.OrdinalIgnoreCase);
		}

		private static string ResolveContextValue(VisionRunContext context, params string[] keys)
		{
			if (context == null || keys == null)
			{
				return string.Empty;
			}

			foreach (string key in keys)
			{
				object value;
				if (!string.IsNullOrWhiteSpace(key) &&
					context.TryGetData(key, out value) &&
					value != null)
				{
					return Convert.ToString(value);
				}
			}

			return string.Empty;
		}

		private static byte[] CopyHalconGrayToByteBuffer(IntPtr pointer, int width, int height, string halconType)
		{
			int pixelCount = width * height;
			string type = (halconType ?? string.Empty).Trim().ToLowerInvariant();

			if (type == "byte" || type.Length == 0)
			{
				byte[] buffer = new byte[pixelCount];
				Marshal.Copy(pointer, buffer, 0, buffer.Length);
				return buffer;
			}

			if (type == "uint2" || type == "int2")
			{
				byte[] raw = new byte[pixelCount * 2];
				Marshal.Copy(pointer, raw, 0, raw.Length);
				double[] values = new double[pixelCount];
				for (int i = 0; i < pixelCount; i++)
				{
					int offset = i * 2;
					if (type == "uint2")
					{
						values[i] = BitConverter.ToUInt16(raw, offset);
					}
					else
					{
						values[i] = BitConverter.ToInt16(raw, offset);
					}
				}

				return NormalizeToByte(values);
			}

			if (type == "real")
			{
				byte[] raw = new byte[pixelCount * 4];
				Marshal.Copy(pointer, raw, 0, raw.Length);
				double[] values = new double[pixelCount];
				for (int i = 0; i < pixelCount; i++)
				{
					values[i] = BitConverter.ToSingle(raw, i * 4);
				}

				return NormalizeToByte(values);
			}

			return null;
		}

		private static Bitmap CreateRgbBitmapFromGray(byte[] gray, int width, int height)
		{
			Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
			BitmapData data = bitmap.LockBits(
				new Rectangle(0, 0, width, height),
				ImageLockMode.WriteOnly,
				PixelFormat.Format24bppRgb);

			try
			{
				int stride = data.Stride;
				byte[] rgb = new byte[stride * height];
				for (int y = 0; y < height; y++)
				{
					int srcOffset = y * width;
					int dstOffset = y * stride;
					for (int x = 0; x < width; x++)
					{
						byte v = gray[srcOffset + x];
						int p = dstOffset + x * 3;
						rgb[p] = v;
						rgb[p + 1] = v;
						rgb[p + 2] = v;
					}
				}

				Marshal.Copy(rgb, 0, data.Scan0, rgb.Length);
			}
			finally
			{
				bitmap.UnlockBits(data);
			}

			return bitmap;
		}

		private static Bitmap CreateRgbBitmapFromChannels(byte[] red, byte[] green, byte[] blue, int width, int height)
		{
			Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
			BitmapData data = bitmap.LockBits(
				new Rectangle(0, 0, width, height),
				ImageLockMode.WriteOnly,
				PixelFormat.Format24bppRgb);

			try
			{
				int stride = data.Stride;
				byte[] rgb = new byte[stride * height];
				for (int y = 0; y < height; y++)
				{
					int srcOffset = y * width;
					int dstOffset = y * stride;
					for (int x = 0; x < width; x++)
					{
						int src = srcOffset + x;
						int dst = dstOffset + x * 3;
						rgb[dst] = blue[src];
						rgb[dst + 1] = green[src];
						rgb[dst + 2] = red[src];
					}
				}

				Marshal.Copy(rgb, 0, data.Scan0, rgb.Length);
			}
			finally
			{
				bitmap.UnlockBits(data);
			}

			return bitmap;
		}

		private static byte[] NormalizeToByte(double[] values)
		{
			if (values == null || values.Length == 0)
			{
				return null;
			}

			double min = double.MaxValue;
			double max = double.MinValue;
			for (int i = 0; i < values.Length; i++)
			{
				double value = values[i];
				if (double.IsNaN(value) || double.IsInfinity(value))
				{
					continue;
				}

				if (value < min) min = value;
				if (value > max) max = value;
			}

			byte[] buffer = new byte[values.Length];
			if (max <= min || min == double.MaxValue)
			{
				return buffer;
			}

			double scale = 255.0 / (max - min);
			for (int i = 0; i < values.Length; i++)
			{
				double scaled = (values[i] - min) * scale;
				if (scaled < 0) scaled = 0;
				if (scaled > 255) scaled = 255;
				buffer[i] = (byte)scaled;
			}

			return buffer;
		}

		private static MethodInfo FindMethod(Type type, string name, int parameterCount)
		{
			if (type == null)
			{
				return null;
			}

			foreach (MethodInfo method in type.GetMethods())
			{
				if (string.Equals(method.Name, name, StringComparison.OrdinalIgnoreCase) &&
					method.GetParameters().Length == parameterCount)
				{
					return method;
				}
			}

			return null;
		}

		private static MethodInfo FindImagePointerMethod(Type type, string methodName, int parameterCount, Type returnType)
		{
			if (type == null)
			{
				return null;
			}

			foreach (MethodInfo method in type.GetMethods())
			{
				if (string.Equals(method.Name, methodName, StringComparison.OrdinalIgnoreCase) &&
					(parameterCount < 0 || method.GetParameters().Length == parameterCount) &&
					(returnType == null || method.ReturnType == returnType))
				{
					return method;
				}
			}

			return null;
		}

		private static MethodInfo FindImagePointer3Method(Type type)
		{
			if (type == null)
			{
				return null;
			}

			MethodInfo fallback = null;
			foreach (MethodInfo method in type.GetMethods())
			{
				if (!string.Equals(method.Name, "GetImagePointer3", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				ParameterInfo[] parameters = method.GetParameters();
				if (parameters.Length != 6)
				{
					continue;
				}

				if (fallback == null)
				{
					fallback = method;
				}

				if (IsByRefElementType(parameters[0].ParameterType, typeof(IntPtr)) &&
					IsByRefElementType(parameters[1].ParameterType, typeof(IntPtr)) &&
					IsByRefElementType(parameters[2].ParameterType, typeof(IntPtr)) &&
					IsByRefElementType(parameters[3].ParameterType, typeof(string)) &&
					IsByRefElementType(parameters[4].ParameterType, typeof(int)) &&
					IsByRefElementType(parameters[5].ParameterType, typeof(int)))
				{
					return method;
				}
			}

			return fallback;
		}

		private static bool IsByRefElementType(Type type, Type elementType)
		{
			return type != null &&
				type.IsByRef &&
				type.GetElementType() == elementType;
		}

		private static MethodInfo FindWriteImageMethod(Type type)
		{
			if (type == null)
			{
				return null;
			}

			foreach (MethodInfo method in type.GetMethods())
			{
				if (!string.Equals(method.Name, "WriteImage", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				ParameterInfo[] parameters = method.GetParameters();
				if (parameters.Length == 3 &&
					IsStringOrHalconTuple(parameters[0].ParameterType) &&
					IsNumericOrHalconTuple(parameters[1].ParameterType) &&
					IsStringOrHalconTuple(parameters[2].ParameterType))
				{
					return method;
				}
			}

			return null;
		}

		private static bool IsStringOrHalconTuple(Type type)
		{
			if (type == null)
			{
				return false;
			}

			return type == typeof(string) ||
				((type.FullName ?? string.Empty).IndexOf("HTuple", StringComparison.OrdinalIgnoreCase) >= 0);
		}

		private static bool IsNumericOrHalconTuple(Type type)
		{
			if (type == null)
			{
				return false;
			}

			return type == typeof(int) ||
				type == typeof(double) ||
				type == typeof(float) ||
				((type.FullName ?? string.Empty).IndexOf("HTuple", StringComparison.OrdinalIgnoreCase) >= 0);
		}

		private static object[] CreateWriteImageArguments(MethodInfo method, string fileName)
		{
			ParameterInfo[] parameters = method.GetParameters();
			return new object[]
			{
				CreateHalconTupleCompatibleValue(parameters[0].ParameterType, "bmp"),
				CreateHalconTupleCompatibleValue(parameters[1].ParameterType, 0),
				CreateHalconTupleCompatibleValue(parameters[2].ParameterType, fileName)
			};
		}

		private static object CreateHalconTupleCompatibleValue(Type targetType, object value)
		{
			if (targetType == null || value == null)
			{
				return value;
			}

			if (targetType.IsInstanceOfType(value))
			{
				return value;
			}

			try
			{
				if (targetType == typeof(int))
				{
					return Convert.ToInt32(value);
				}

				if (targetType == typeof(string))
				{
					return Convert.ToString(value);
				}

				if ((targetType.FullName ?? string.Empty).IndexOf("HTuple", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					return Activator.CreateInstance(targetType, new object[] { value });
				}
			}
			catch
			{
			}

			return value;
		}

		private static Type FindType(string typeName)
		{
			Type type = Type.GetType(typeName + ", HalconDotNet", false);
			if (type != null)
			{
				return type;
			}

			try
			{
				Assembly assembly = Assembly.Load("HalconDotNet");
				type = assembly.GetType(typeName, false, true);
				if (type != null)
				{
					return type;
				}
			}
			catch
			{
			}

			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				try
				{
					type = assembly.GetType(typeName, false, true);
					if (type != null)
					{
						return type;
					}
				}
				catch
				{
				}
			}

			return null;
		}
	}
}
