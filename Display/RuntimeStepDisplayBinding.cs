using System;
using System.Drawing;
using System.Reflection;

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

			if (rawImage == null)
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

			return null;
		}
	}
}
