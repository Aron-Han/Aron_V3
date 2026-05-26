using System;
using System.Drawing;
using System.Drawing.Imaging;
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

			if (rawImage == null && step.StepType != StepType.Halcon)
			{
				rawImage = TryGetFirstInputImage(step, context);
				selectedImage = null;
			}

			Bitmap bitmap = ImageConvertHelper.TryConvertToBitmap(rawImage);

			if (bitmap == null)
			{
				return;
			}

			try
			{
				string sourceInfo = jobName + " / " + taskName + " / " + step.StepName;

				DisplayRuntimeManager.ShowImage(
					step.DisplaySlotName,
					bitmap,
					sourceInfo,
					step.DisplayMode,
					selectedImage == null ? null : selectedImage.DisplayRecord,
					selectedImage == null ? string.Empty : selectedImage.DisplayRecordKey);
			}
			finally
			{
				bitmap.Dispose();
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

				MethodInfo pointerMethod = FindImagePointerMethod(imageObject.GetType());
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

		private static MethodInfo FindImagePointerMethod(Type type)
		{
			if (type == null)
			{
				return null;
			}

			foreach (MethodInfo method in type.GetMethods())
			{
				if (string.Equals(method.Name, "GetImagePointer1", StringComparison.OrdinalIgnoreCase) &&
					method.GetParameters().Length == 3 &&
					method.ReturnType == typeof(IntPtr))
				{
					return method;
				}
			}

			return null;
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
