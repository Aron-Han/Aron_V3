using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;
using Cognex.VisionPro;
using Cognex.VisionPro.Implementation;

namespace Aron_V3
{
	internal static class VisionProRecordBitmapRenderer
	{
		public static Bitmap TryRender(object recordValue, string recordKey, Size displayArea)
		{
			ICogRecord record = recordValue as ICogRecord;
			if (record == null)
			{
				return null;
			}

			ICogRecord imageRecord = FindImageRecord(record, recordKey);
			ICogImage image = imageRecord == null ? null : imageRecord.Content as ICogImage;
			if (image == null)
			{
				return null;
			}

			Bitmap sourceBitmap = VisionProImageBitmapConverter.TryConvertToBitmap(image) ??
				ImageConvertHelper.TryConvertToBitmap(image);
			if (sourceBitmap == null)
			{
				return null;
			}

			try
			{
				float displayScale = CalculateDisplayScale(sourceBitmap.Size, displayArea);

				List<GraphicItem> items = new List<GraphicItem>();
				CollectGraphics(record, imageRecord, image, items);

				using (Graphics graphics = Graphics.FromImage(sourceBitmap))
				{
					graphics.SmoothingMode = SmoothingMode.AntiAlias;
					graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

					foreach (GraphicItem item in items)
					{
						DrawGraphic(graphics, item, displayScale);
					}
				}

				Bitmap result = sourceBitmap;
				sourceBitmap = null;
				return result;
			}
			catch
			{
				return null;
			}
			finally
			{
				if (sourceBitmap != null)
				{
					sourceBitmap.Dispose();
				}
			}
		}

		private static float CalculateDisplayScale(Size imageSize, Size displayArea)
		{
			if (imageSize.Width <= 0 || imageSize.Height <= 0 ||
				displayArea.Width <= 0 || displayArea.Height <= 0)
			{
				return 1F;
			}

			float horizontalScale = displayArea.Width / (float)imageSize.Width;
			float verticalScale = displayArea.Height / (float)imageSize.Height;
			float scale = Math.Min(horizontalScale, verticalScale);
			return scale > 0F ? scale : 1F;
		}

		private static ICogRecord FindImageRecord(ICogRecord record, string recordKey)
		{
			ICogRecord keyedRecord = FindRecordByKey(record, recordKey);
			if (keyedRecord != null && keyedRecord.Content is ICogImage)
			{
				return keyedRecord;
			}

			return FindFirstImageRecord(record);
		}

		private static ICogRecord FindFirstImageRecord(ICogRecord record)
		{
			if (record == null)
			{
				return null;
			}

			if (record.Content is ICogImage)
			{
				return record;
			}

			foreach (ICogRecord subRecord in record.SubRecords)
			{
				ICogRecord found = FindFirstImageRecord(subRecord);
				if (found != null)
				{
					return found;
				}
			}

			return null;
		}

		private static ICogRecord FindRecordByKey(ICogRecord record, string recordKey)
		{
			if (record == null || string.IsNullOrWhiteSpace(recordKey))
			{
				return null;
			}

			string currentKey = record.RecordKey;
			if (string.Equals(currentKey, recordKey, StringComparison.OrdinalIgnoreCase) ||
				(!string.IsNullOrWhiteSpace(currentKey) && currentKey.EndsWith("." + recordKey, StringComparison.OrdinalIgnoreCase)))
			{
				return record;
			}

			foreach (ICogRecord subRecord in record.SubRecords)
			{
				ICogRecord found = FindRecordByKey(subRecord, recordKey);
				if (found != null)
				{
					return found;
				}
			}

			return null;
		}

		private static void CollectGraphics(ICogRecord rootRecord, ICogRecord imageRecord, ICogImage image, List<GraphicItem> items)
		{
			if (imageRecord != null)
			{
				CollectAllGraphics(imageRecord, image, items);
			}

			if (items.Count > 0)
			{
				return;
			}

			string scopeKey = GetRecordScopeKey(imageRecord);
			if (!string.IsNullOrWhiteSpace(scopeKey))
			{
				CollectGraphicsInScope(rootRecord, scopeKey, image, items);
			}
		}

		private static string GetRecordScopeKey(ICogRecord imageRecord)
		{
			string key = imageRecord == null ? string.Empty : (imageRecord.RecordKey ?? string.Empty).Trim();
			if (string.IsNullOrWhiteSpace(key))
			{
				return string.Empty;
			}

			if (key.EndsWith(".OutputImage", StringComparison.OrdinalIgnoreCase))
			{
				return key.Substring(0, key.Length - ".OutputImage".Length);
			}

			return key;
		}

		private static void CollectGraphicsInScope(ICogRecord record, string scopeKey, ICogImage image, List<GraphicItem> items)
		{
			if (record == null)
			{
				return;
			}

			string recordKey = record.RecordKey ?? string.Empty;
			bool inScope = string.Equals(recordKey, scopeKey, StringComparison.OrdinalIgnoreCase) ||
				(!string.IsNullOrWhiteSpace(recordKey) && recordKey.StartsWith(scopeKey + ".", StringComparison.OrdinalIgnoreCase));

			if (inScope)
			{
				CollectContent(record.Content, image, items, string.Empty);
			}

			foreach (ICogRecord subRecord in record.SubRecords)
			{
				CollectGraphicsInScope(subRecord, scopeKey, image, items);
			}
		}

		private static void CollectAllGraphics(ICogRecord record, ICogImage image, List<GraphicItem> items)
		{
			foreach (ICogRecord subRecord in record.SubRecords)
			{
				CollectContent(subRecord.Content, image, items, string.Empty);
				CollectAllGraphics(subRecord, image, items);
			}
		}

		private static void CollectContent(object content, ICogImage image, List<GraphicItem> items, string inheritedSpaceName)
		{
			if (content == null)
			{
				return;
			}

			CogGraphicCollection collection = content as CogGraphicCollection;
			if (collection != null)
			{
				foreach (ICogGraphic graphic in collection)
				{
					CollectContent(graphic, image, items, inheritedSpaceName);
				}

				return;
			}

			CogCompositeShape composite = content as CogCompositeShape;
			if (composite != null)
			{
				string compositeSpaceName = string.IsNullOrWhiteSpace(composite.SelectedSpaceName)
					? inheritedSpaceName
					: composite.SelectedSpaceName;
				foreach (ICogGraphic child in composite.Children)
				{
					CollectContent(child, image, items, compositeSpaceName);
				}

				return;
			}

			ICogGraphic item = content as ICogGraphic;
			if (item != null)
			{
				TransformInfo transform = TryGetTransform(image, item, inheritedSpaceName);
				if (transform.Resolved)
				{
					items.Add(new GraphicItem(item, transform.Transform));
				}
			}
		}

		private static TransformInfo TryGetTransform(ICogImage image, ICogGraphic graphic, string inheritedSpaceName)
		{
			string spaceName = graphic.SelectedSpaceName;

			if (string.Equals(spaceName, "$", StringComparison.Ordinal))
			{
				if (!string.IsNullOrWhiteSpace(inheritedSpaceName))
				{
					spaceName = inheritedSpaceName;
				}

				try
				{
					PropertyInfo parentProperty = graphic.GetType().GetProperty("Parent", BindingFlags.Instance | BindingFlags.Public);
					CogCompositeShape parent = parentProperty == null ? null : parentProperty.GetValue(graphic, null) as CogCompositeShape;
					if (parent != null)
					{
						spaceName = parent.SelectedSpaceName;
					}
				}
				catch
				{
				}
			}

			if (string.IsNullOrWhiteSpace(spaceName) || string.Equals(spaceName, "#", StringComparison.Ordinal))
			{
				return new TransformInfo(null, true);
			}

			try
			{
				return new TransformInfo(image.GetTransform("#", spaceName), true);
			}
			catch
			{
				return new TransformInfo(null, false);
			}
		}

		private static void DrawGraphic(Graphics graphics, GraphicItem item, float displayScale)
		{
			if (item == null || item.Graphic == null || !item.Graphic.Visible)
			{
				return;
			}

			GraphicsState state = graphics.Save();
			try
			{
				using (Matrix transform = CreateMatrix(item.Transform))
				{
					CogPointMarker pointMarker = item.Graphic as CogPointMarker;
					if (pointMarker != null)
					{
						DrawPointMarker(graphics, pointMarker, transform, displayScale);
						return;
					}

					CogCoordinateAxes axes = item.Graphic as CogCoordinateAxes;
					if (axes != null)
					{
						DrawCoordinateAxes(graphics, axes, transform, displayScale);
						return;
					}

					CogGraphicLabel label = item.Graphic as CogGraphicLabel;
					if (label != null)
					{
						DrawLabel(graphics, label, transform, displayScale);
						return;
					}

					CogLineSegment line = item.Graphic as CogLineSegment;
					if (line != null)
					{
						if (!IsAbnormalDisplayLine(line))
						{
							DrawLineSegment(graphics, line, transform, displayScale);
						}
						return;
					}

					CogRectangleAffine affineRectangle = item.Graphic as CogRectangleAffine;
					if (affineRectangle != null)
					{
						DrawAffineRectangle(graphics, affineRectangle, transform, displayScale);
						return;
					}

					CogRectangle rectangle = item.Graphic as CogRectangle;
					if (rectangle != null)
					{
						DrawRectangle(graphics, rectangle, transform, displayScale);
						return;
					}

					CogCircularAnnulusSection circularSection = item.Graphic as CogCircularAnnulusSection;
					if (circularSection != null)
					{
						DrawCircularAnnulusSection(graphics, circularSection, transform, displayScale);
						return;
					}

					CogCircularArc arc = item.Graphic as CogCircularArc;
					if (arc != null)
					{
						DrawCircularArc(graphics, arc, transform, displayScale);
						return;
					}

					CogCircle circle = item.Graphic as CogCircle;
					if (circle != null)
					{
						DrawCircle(graphics, circle, transform, displayScale);
						return;
					}

					CogPolygon polygon = item.Graphic as CogPolygon;
					if (polygon != null)
					{
						DrawPolygon(graphics, polygon, transform, displayScale);
						return;
					}

					CogEllipticalAnnulusSection ellipticalSection = item.Graphic as CogEllipticalAnnulusSection;
					if (ellipticalSection != null)
					{
						DrawEllipticalAnnulusSection(graphics, ellipticalSection, transform, displayScale);
						return;
					}

					CogEllipticalArc ellipticalArc = item.Graphic as CogEllipticalArc;
					if (ellipticalArc != null)
					{
						DrawEllipticalArc(graphics, ellipticalArc, transform, displayScale);
						return;
					}

					CogEllipse ellipse = item.Graphic as CogEllipse;
					if (ellipse != null)
					{
						DrawEllipse(graphics, ellipse, transform, displayScale);
						return;
					}
				}
			}
			finally
			{
				graphics.Restore(state);
			}
		}

		private static void DrawLineSegment(Graphics graphics, CogLineSegment line, Matrix transform, float displayScale)
		{
			PointF[] points =
			{
				new PointF((float)line.StartX, (float)line.StartY),
				new PointF((float)line.EndX, (float)line.EndY)
			};

			transform.TransformPoints(points);
			using (Pen pen = CreatePen(line.Color, line.LineWidthInScreenPixels, displayScale))
			{
				graphics.DrawLine(pen, points[0], points[1]);
			}
		}

		private static void DrawRectangle(Graphics graphics, CogRectangle rectangle, Matrix transform, float displayScale)
		{
			using (Pen pen = CreatePen(rectangle.Color, rectangle.LineWidthInScreenPixels, displayScale))
			using (GraphicsPath path = new GraphicsPath())
			{
				path.AddRectangle(new RectangleF(
					(float)rectangle.X,
					(float)rectangle.Y,
					(float)rectangle.Width,
					(float)rectangle.Height));
				path.Transform(transform);
				graphics.DrawPath(pen, path);
			}
		}

		private static void DrawCircle(Graphics graphics, CogCircle circle, Matrix transform, float displayScale)
		{
			using (Pen pen = CreatePen(circle.Color, circle.LineWidthInScreenPixels, displayScale))
			using (GraphicsPath path = new GraphicsPath())
			{
				float radius = (float)circle.Radius;
				path.AddEllipse(
					(float)circle.CenterX - radius,
					(float)circle.CenterY - radius,
					radius * 2F,
					radius * 2F);
				path.Transform(transform);
				graphics.DrawPath(pen, path);
			}
		}

		private static void DrawPointMarker(Graphics graphics, CogPointMarker pointMarker, Matrix transform, float displayScale)
		{
			float screenSize = ClampScreenValue((float)pointMarker.SizeInScreenPixels, 8F, 45F);
			float halfLength = displayScale > 0F
				? screenSize / displayScale / 2F
				: screenSize / 2F;

			GraphicsState state = graphics.Save();
			try
			{
				PointF center = TransformPoint(transform, pointMarker.X, pointMarker.Y);
				float rotation = GetTransformedRotationDegrees(
					transform,
					pointMarker.X,
					pointMarker.Y,
					pointMarker.Rotation);
				graphics.TranslateTransform(center.X, center.Y);
				graphics.RotateTransform(rotation);
				using (Pen pen = CreateScreenPen(pointMarker.Color, pointMarker.LineWidthInScreenPixels, displayScale))
				{
					graphics.DrawLine(pen, -halfLength, 0F, halfLength, 0F);
					graphics.DrawLine(pen, 0F, -halfLength, 0F, halfLength);
				}
			}
			finally
			{
				graphics.Restore(state);
			}
		}

		private static void DrawCoordinateAxes(Graphics graphics, CogCoordinateAxes axes, Matrix transform, float displayScale)
		{
			float screenLength = ClampScreenValue((float)axes.DisplayedXAxisLength, 12F, 55F);
			float length = displayScale > 0F
				? screenLength / displayScale
				: screenLength;
			float arrowLength = displayScale > 0F ? 10F / displayScale : 10F;

			GraphicsState state = graphics.Save();
			try
			{
				PointF origin;
				PointF xDirection;
				PointF yDirection;
				ResolveCoordinateAxesDirections(axes, transform, out origin, out xDirection, out yDirection);

				graphics.TranslateTransform(origin.X, origin.Y);
				using (Pen pen = CreateScreenPen(axes.Color, axes.LineWidthInScreenPixels, displayScale))
				{
					PointF xEnd = ScaleDirection(xDirection, length);
					PointF yEnd = ScaleDirection(yDirection, length);
					graphics.DrawLine(pen, PointF.Empty, xEnd);
					graphics.DrawLine(pen, PointF.Empty, yEnd);
					DrawArrow(graphics, pen, xEnd.X, xEnd.Y, arrowLength);
					DrawArrow(graphics, pen, yEnd.X, yEnd.Y, arrowLength);

					float fontSize = displayScale > 0F ? 10F / displayScale : 10F;
					using (Font font = new Font("Microsoft YaHei UI", fontSize, FontStyle.Regular, GraphicsUnit.Point))
					using (Brush brush = new SolidBrush(ToColor(axes.Color)))
					{
						PointF xLabel = OffsetPoint(xEnd, xDirection, arrowLength);
						PointF yLabel = OffsetPoint(yEnd, yDirection, arrowLength);
						graphics.DrawString("X", font, brush, xLabel);
						graphics.DrawString("Y", font, brush, yLabel);
					}
				}
			}
			finally
			{
				graphics.Restore(state);
			}
		}

		private static void ResolveCoordinateAxesDirections(
			CogCoordinateAxes axes,
			Matrix transform,
			out PointF origin,
			out PointF xDirection,
			out PointF yDirection)
		{
			double originX;
			double originY;
			double xAxisX;
			double xAxisY;
			double yAxisX;
			double yAxisY;
			axes.GetOriginCornerXCornerY(
				out originX,
				out originY,
				out xAxisX,
				out xAxisY,
				out yAxisX,
				out yAxisY);

			origin = TransformPoint(transform, originX, originY);
			PointF xAxis = TransformPoint(transform, xAxisX, xAxisY);
			PointF yAxis = TransformPoint(transform, yAxisX, yAxisY);

			xDirection = NormalizeDirection(origin, xAxis, 1F, 0F);
			yDirection = NormalizeDirection(origin, yAxis, 0F, 1F);
		}

		private static PointF NormalizeDirection(PointF origin, PointF endpoint, float fallbackX, float fallbackY)
		{
			float dx = endpoint.X - origin.X;
			float dy = endpoint.Y - origin.Y;
			float length = (float)Math.Sqrt(dx * dx + dy * dy);
			if (length <= 0.0001F)
			{
				return new PointF(fallbackX, fallbackY);
			}

			return new PointF(dx / length, dy / length);
		}

		private static PointF ScaleDirection(PointF direction, float length)
		{
			return new PointF(direction.X * length, direction.Y * length);
		}

		private static PointF OffsetPoint(PointF point, PointF direction, float offset)
		{
			return new PointF(point.X + direction.X * offset, point.Y + direction.Y * offset);
		}

		private static void DrawArrow(Graphics graphics, Pen pen, float endX, float endY, float arrowLength)
		{
			float angle = (float)Math.Atan2(endY, endX);
			float arrowAngle = 25F * (float)Math.PI / 180F;
			float backAngle = (float)Math.PI;

			float first = angle + backAngle - arrowAngle;
			float second = angle + backAngle + arrowAngle;

			graphics.DrawLine(
				pen,
				endX,
				endY,
				endX + arrowLength * (float)Math.Cos(first),
				endY + arrowLength * (float)Math.Sin(first));
			graphics.DrawLine(
				pen,
				endX,
				endY,
				endX + arrowLength * (float)Math.Cos(second),
				endY + arrowLength * (float)Math.Sin(second));
		}

		private static void DrawCircularArc(Graphics graphics, CogCircularArc arc, Matrix transform, float displayScale)
		{
			using (Pen pen = CreatePen(arc.Color, arc.LineWidthInScreenPixels, displayScale))
			using (GraphicsPath path = new GraphicsPath())
			{
				path.AddArc(
					(float)(arc.CenterX - arc.Radius),
					(float)(arc.CenterY - arc.Radius),
					(float)(arc.Radius * 2),
					(float)(arc.Radius * 2),
					(float)(arc.AngleStart * 180.0 / Math.PI),
					(float)(arc.AngleSpan * 180.0 / Math.PI));
				path.Transform(transform);
				graphics.DrawPath(pen, path);
			}
		}

		private static void DrawCircularAnnulusSection(Graphics graphics, CogCircularAnnulusSection section, Matrix transform, float displayScale)
		{
			double radius1 = section.Radius;
			double radius2 = section.Radius * section.RadialScale;
			double outerRadius = Math.Max(radius1, radius2);
			double innerRadius = Math.Min(radius1, radius2);
			double start = section.AngleStart;
			double end = section.AngleStart + section.AngleSpan;

			using (Pen pen = CreatePen(section.Color, section.LineWidthInScreenPixels, displayScale))
			using (GraphicsPath path = new GraphicsPath())
			{
				AddCircularArcPoints(path, section.CenterX, section.CenterY, outerRadius, start, end, true);
				AddCircularArcPoints(path, section.CenterX, section.CenterY, innerRadius, end, start, false);
				path.CloseFigure();
				path.Transform(transform);
				graphics.DrawPath(pen, path);
			}
		}

		private static void DrawEllipticalArc(Graphics graphics, CogEllipticalArc arc, Matrix transform, float displayScale)
		{
			using (Pen pen = CreatePen(arc.Color, arc.LineWidthInScreenPixels, displayScale))
			using (GraphicsPath path = new GraphicsPath())
			using (Matrix local = CreateEllipseLocalMatrix(arc.CenterX, arc.CenterY, arc.Rotation))
			{
				AddEllipseArcPoints(path, arc.RadiusX, arc.RadiusY, arc.AngleStart, arc.AngleStart + arc.AngleSpan, true);
				path.Transform(local);
				path.Transform(transform);
				graphics.DrawPath(pen, path);
			}
		}

		private static void DrawEllipticalAnnulusSection(Graphics graphics, CogEllipticalAnnulusSection section, Matrix transform, float displayScale)
		{
			double radiusX1 = section.RadiusX;
			double radiusY1 = section.RadiusY;
			double radiusX2 = section.RadiusX * section.RadialScale;
			double radiusY2 = section.RadiusY * section.RadialScale;

			double outerRadiusX = Math.Max(Math.Abs(radiusX1), Math.Abs(radiusX2));
			double outerRadiusY = Math.Max(Math.Abs(radiusY1), Math.Abs(radiusY2));
			double innerRadiusX = Math.Min(Math.Abs(radiusX1), Math.Abs(radiusX2));
			double innerRadiusY = Math.Min(Math.Abs(radiusY1), Math.Abs(radiusY2));
			double start = section.AngleStart;
			double end = section.AngleStart + section.AngleSpan;

			using (Pen pen = CreatePen(section.Color, section.LineWidthInScreenPixels, displayScale))
			using (GraphicsPath path = new GraphicsPath())
			using (Matrix local = CreateEllipseLocalMatrix(section.CenterX, section.CenterY, section.Rotation))
			{
				AddEllipseArcPoints(path, outerRadiusX, outerRadiusY, start, end, true);
				AddEllipseArcPoints(path, innerRadiusX, innerRadiusY, end, start, false);
				path.CloseFigure();
				path.Transform(local);
				path.Transform(transform);
				graphics.DrawPath(pen, path);
			}
		}

		private static Matrix CreateEllipseLocalMatrix(double centerX, double centerY, double rotation)
		{
			Matrix matrix = new Matrix();
			matrix.Rotate((float)(rotation * 180.0 / Math.PI), MatrixOrder.Append);
			matrix.Translate((float)centerX, (float)centerY, MatrixOrder.Append);
			return matrix;
		}

		private static void AddCircularArcPoints(GraphicsPath path, double centerX, double centerY, double radius, double start, double end, bool startFigure)
		{
			int count = CalculateSegmentCount(radius, radius, start, end);
			for (int index = 0; index <= count; index++)
			{
				double t = start + (end - start) * index / count;
				float x = (float)(centerX + radius * Math.Cos(t));
				float y = (float)(centerY + radius * Math.Sin(t));

				if (index == 0 && startFigure)
				{
					path.StartFigure();
					path.AddLine(x, y, x, y);
				}
				else
				{
					PointF last = path.GetLastPoint();
					path.AddLine(last.X, last.Y, x, y);
				}
			}
		}

		private static void AddEllipseArcPoints(GraphicsPath path, double radiusX, double radiusY, double start, double end, bool startFigure)
		{
			int count = CalculateSegmentCount(radiusX, radiusY, start, end);
			for (int index = 0; index <= count; index++)
			{
				double t = start + (end - start) * index / count;
				PointF point = EllipseRayIntersect(radiusX, radiusY, t);

				if (index == 0 && startFigure)
				{
					path.StartFigure();
					path.AddLine(point.X, point.Y, point.X, point.Y);
				}
				else
				{
					PointF last = path.GetLastPoint();
					path.AddLine(last.X, last.Y, point.X, point.Y);
				}
			}
		}

		private static PointF EllipseRayIntersect(double radiusX, double radiusY, double angle)
		{
			double a = Math.Abs(radiusX);
			double b = Math.Abs(radiusY);
			if (a <= 0.000001 || b <= 0.000001)
			{
				return new PointF(0F, 0F);
			}

			double cos = Math.Cos(angle);
			double sin = Math.Sin(angle);
			double distance = 1.0 / Math.Sqrt((cos * cos) / (a * a) + (sin * sin) / (b * b));
			return new PointF((float)(distance * cos), (float)(distance * sin));
		}

		private static int CalculateSegmentCount(double radiusX, double radiusY, double start, double end)
		{
			double radius = Math.Max(Math.Abs(radiusX), Math.Abs(radiusY));
			double span = Math.Abs(end - start);
			int count = (int)Math.Ceiling(span * Math.Max(8.0, radius) / 8.0);
			if (count < 16)
			{
				return 16;
			}

			if (count > 256)
			{
				return 256;
			}

			return count;
		}

		private static void DrawAffineRectangle(Graphics graphics, CogRectangleAffine rectangle, Matrix transform, float displayScale)
		{
			double originX;
			double originY;
			double cornerXX;
			double cornerXY;
			double cornerYX;
			double cornerYY;
			rectangle.GetOriginCornerXCornerY(
				out originX,
				out originY,
				out cornerXX,
				out cornerXY,
				out cornerYX,
				out cornerYY);

			PointF[] points =
			{
				new PointF((float)originX, (float)originY),
				new PointF((float)cornerXX, (float)cornerXY),
				new PointF((float)(cornerXX + cornerYX - originX), (float)(cornerXY + cornerYY - originY)),
				new PointF((float)cornerYX, (float)cornerYY)
			};

			using (Pen pen = CreatePen(rectangle.Color, rectangle.LineWidthInScreenPixels, displayScale))
			{
				transform.TransformPoints(points);
				graphics.DrawPolygon(pen, points);
			}
		}

		private static void DrawPolygon(Graphics graphics, CogPolygon polygon, Matrix transform, float displayScale)
		{
			double[,] vertices = polygon.GetVertices();
			int count = vertices.GetLength(0);
			if (count < 2)
			{
				return;
			}

			PointF[] points = new PointF[count];
			for (int index = 0; index < count; index++)
			{
				points[index] = new PointF((float)vertices[index, 0], (float)vertices[index, 1]);
			}

			using (Pen pen = CreatePen(polygon.Color, polygon.LineWidthInScreenPixels, displayScale))
			{
				transform.TransformPoints(points);
				if (count == 2)
				{
					graphics.DrawLines(pen, points);
				}
				else
				{
					graphics.DrawPolygon(pen, points);
				}
			}
		}

		private static void DrawEllipse(Graphics graphics, CogEllipse ellipse, Matrix transform, float displayScale)
		{
			using (GraphicsPath path = new GraphicsPath())
			{
				using (Pen pen = CreatePen(ellipse.Color, ellipse.LineWidthInScreenPixels, displayScale))
				{
					using (Matrix local = CreateEllipseLocalMatrix(ellipse.CenterX, ellipse.CenterY, ellipse.Rotation))
					{
						path.AddEllipse(
							(float)-ellipse.RadiusX,
							(float)-ellipse.RadiusY,
							(float)(ellipse.RadiusX * 2),
							(float)(ellipse.RadiusY * 2));
						path.Transform(local);
						path.Transform(transform);
					}

					graphics.DrawPath(pen, path);
				}
			}
		}

		private static void DrawLabel(Graphics graphics, CogGraphicLabel label, Matrix transform, float displayScale)
		{
			if (string.IsNullOrEmpty(label.Text))
			{
				return;
			}

			GraphicsState state = graphics.Save();
			try
			{
				PointF location = TransformPoint(transform, label.X, label.Y);
				graphics.TranslateTransform(location.X, location.Y);
				graphics.RotateTransform(GetRotationDegrees(transform) + (float)(label.Rotation * 180.0 / Math.PI));
				using (Brush brush = new SolidBrush(ToColor(label.Color)))
				{
					Font baseFont = label.Font ?? SystemFonts.DefaultFont;
					float adjustedSize = displayScale > 0F ? baseFont.Size / displayScale : baseFont.Size;
					using (Font font = new Font(baseFont.FontFamily, adjustedSize, baseFont.Style, GraphicsUnit.Point))
					{
						graphics.DrawString(label.Text, font, brush, PointF.Empty);
					}
				}
			}
			finally
			{
				graphics.Restore(state);
			}
		}

		private static PointF TransformPoint(Matrix transform, double x, double y)
		{
			PointF[] points = { new PointF((float)x, (float)y) };
			transform.TransformPoints(points);
			return points[0];
		}

		private static float GetRotationDegrees(Matrix transform)
		{
			float[] elements = transform.Elements;
			return (float)(Math.Atan2(elements[1], elements[0]) * 180.0 / Math.PI);
		}

		private static float GetTransformedRotationDegrees(Matrix transform, double originX, double originY, double rotation)
		{
			PointF direction = TransformDirection(transform, originX, originY, rotation);
			return (float)(Math.Atan2(direction.Y, direction.X) * 180.0 / Math.PI);
		}

		private static PointF TransformDirection(Matrix transform, double originX, double originY, double rotation)
		{
			PointF origin = TransformPoint(transform, originX, originY);
			PointF endpoint = TransformPoint(
				transform,
				originX + Math.Cos(rotation),
				originY + Math.Sin(rotation));

			float dx = endpoint.X - origin.X;
			float dy = endpoint.Y - origin.Y;
			float length = (float)Math.Sqrt(dx * dx + dy * dy);
			if (length <= 0.0001F)
			{
				return new PointF(1F, 0F);
			}

			return new PointF(dx / length, dy / length);
		}

		private static Pen CreatePen(CogColorConstants color, double width, float displayScale)
		{
			float screenWidth = ClampScreenValue((float)width, 1F, 3F);
			float adjustedWidth = displayScale > 0F ? screenWidth / displayScale : screenWidth;
			return new Pen(ToColor(color), Math.Max(1F, adjustedWidth));
		}

		private static Pen CreateScreenPen(CogColorConstants color, double width, float displayScale)
		{
			float screenWidth = ClampScreenValue((float)width, 1F, 3F);
			float adjustedWidth = displayScale > 0F ? screenWidth / displayScale : screenWidth;
			return new Pen(ToColor(color), Math.Max(1F, adjustedWidth));
		}

		private static float ClampScreenValue(float value, float min, float max)
		{
			if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0F)
			{
				return min;
			}

			if (value < min)
			{
				return min;
			}

			if (value > max)
			{
				return max;
			}

			return value;
		}

		private static bool IsAbnormalDisplayLine(CogLineSegment line)
		{
			if (line == null)
			{
				return true;
			}

			double dx = line.EndX - line.StartX;
			double dy = line.EndY - line.StartY;
			double length = Math.Sqrt(dx * dx + dy * dy);

			if (double.IsNaN(length) || double.IsInfinity(length))
			{
				return true;
			}

			return length > 5000.0;
		}

		private static bool IsUnwantedAuxiliaryGraphic(ICogGraphic graphic)
		{
			return graphic != null && graphic.Color == CogColorConstants.Yellow;
		}

		private static Color ToColor(CogColorConstants color)
		{
			switch (color)
			{
				case CogColorConstants.None:
					return Color.Transparent;
				case CogColorConstants.Black:
					return Color.Black;
				case CogColorConstants.White:
					return Color.White;
				case CogColorConstants.Red:
					return Color.Red;
				case CogColorConstants.DarkRed:
					return Color.DarkRed;
				case CogColorConstants.Green:
					return Color.Lime;
				case CogColorConstants.DarkGreen:
					return Color.DarkGreen;
				case CogColorConstants.Blue:
					return Color.Blue;
				case CogColorConstants.Cyan:
					return Color.Cyan;
				case CogColorConstants.Magenta:
					return Color.Magenta;
				case CogColorConstants.Purple:
					return Color.Purple;
				case CogColorConstants.Yellow:
					return Color.Yellow;
				case CogColorConstants.Orange:
					return Color.Orange;
				case CogColorConstants.LightGrey:
					return Color.LightGray;
				case CogColorConstants.Grey:
					return Color.Gray;
				case CogColorConstants.DarkGrey:
					return Color.DarkGray;
				default:
					return Color.Magenta;
			}
		}

		private static Matrix CreateMatrix(ICogTransform2D transform)
		{
			CogTransform2DLinear linear = transform as CogTransform2DLinear;
			if (linear == null)
			{
				return new Matrix();
			}

			return new Matrix(
				(float)linear.GetMatrixElement(0, 0),
				(float)linear.GetMatrixElement(1, 0),
				(float)linear.GetMatrixElement(0, 1),
				(float)linear.GetMatrixElement(1, 1),
				(float)linear.TranslationX,
				(float)linear.TranslationY);
		}

		private sealed class GraphicItem
		{
			public ICogGraphic Graphic { get; private set; }
			public ICogTransform2D Transform { get; private set; }

			public GraphicItem(ICogGraphic graphic, ICogTransform2D transform)
			{
				Graphic = graphic;
				Transform = transform;
			}
		}

		private sealed class TransformInfo
		{
			public ICogTransform2D Transform { get; private set; }
			public bool Resolved { get; private set; }

			public TransformInfo(ICogTransform2D transform, bool resolved)
			{
				Transform = transform;
				Resolved = resolved;
			}
		}
	}

	internal static class VisionProImageBitmapConverter
	{
		public static Bitmap TryConvertToBitmap(object image)
		{
			return TryConvertToBitmap(image as ICogImage);
		}

		public static Bitmap TryConvertToBitmap(ICogImage image)
		{
			if (image == null)
			{
				return null;
			}

			CogImage8Grey grey = image as CogImage8Grey;
			if (grey != null)
			{
				return TryConvertGray8(grey);
			}

			CogImage24PlanarColor color = image as CogImage24PlanarColor;
			if (color != null)
			{
				return TryConvertColor24(color);
			}

			return null;
		}

		private static Bitmap TryConvertGray8(CogImage8Grey image)
		{
			if (!HasValidSize(image.Width, image.Height))
			{
				return null;
			}

			Bitmap bitmap = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb);

			try
			{
				ICogImage8PixelMemory source = image.Get8GreyPixelMemory(
					CogImageDataModeConstants.Read,
					0,
					0,
					image.Width,
					image.Height);

				BitmapData data = null;
				try
				{
					data = bitmap.LockBits(
						new Rectangle(0, 0, image.Width, image.Height),
						ImageLockMode.WriteOnly,
						PixelFormat.Format32bppArgb);

					byte[] sourceRow = new byte[image.Width];
					byte[] targetRow = new byte[image.Width * 4];

					for (int y = 0; y < image.Height; y++)
					{
						Marshal.Copy(IntPtr.Add(source.Scan0, y * source.Stride), sourceRow, 0, image.Width);
						CopyGrayRowToBgra(sourceRow, targetRow, image.Width);
						Marshal.Copy(targetRow, 0, IntPtr.Add(data.Scan0, y * data.Stride), targetRow.Length);
					}
				}
				finally
				{
					if (data != null)
					{
						bitmap.UnlockBits(data);
					}
				}

				return bitmap;
			}
			catch
			{
				bitmap.Dispose();
				return null;
			}
		}

		private static Bitmap TryConvertColor24(CogImage24PlanarColor image)
		{
			if (!HasValidSize(image.Width, image.Height))
			{
				return null;
			}

			Bitmap bitmap = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb);

			try
			{
				ICogImage8PixelMemory red;
				ICogImage8PixelMemory green;
				ICogImage8PixelMemory blue;
				image.Get24PlanarColorPixelMemory(
					CogImageDataModeConstants.Read,
					0,
					0,
					image.Width,
					image.Height,
					out red,
					out green,
					out blue);

				BitmapData data = null;
				try
				{
					data = bitmap.LockBits(
						new Rectangle(0, 0, image.Width, image.Height),
						ImageLockMode.WriteOnly,
						PixelFormat.Format32bppArgb);

					byte[] redRow = new byte[image.Width];
					byte[] greenRow = new byte[image.Width];
					byte[] blueRow = new byte[image.Width];
					byte[] targetRow = new byte[image.Width * 4];

					for (int y = 0; y < image.Height; y++)
					{
						Marshal.Copy(IntPtr.Add(red.Scan0, y * red.Stride), redRow, 0, image.Width);
						Marshal.Copy(IntPtr.Add(green.Scan0, y * green.Stride), greenRow, 0, image.Width);
						Marshal.Copy(IntPtr.Add(blue.Scan0, y * blue.Stride), blueRow, 0, image.Width);
						CopyPlanarColorRowToBgra(redRow, greenRow, blueRow, targetRow, image.Width);
						Marshal.Copy(targetRow, 0, IntPtr.Add(data.Scan0, y * data.Stride), targetRow.Length);
					}
				}
				finally
				{
					if (data != null)
					{
						bitmap.UnlockBits(data);
					}
				}

				return bitmap;
			}
			catch
			{
				bitmap.Dispose();
				return null;
			}
		}

		private static bool HasValidSize(int width, int height)
		{
			return width > 0 && height > 0;
		}

		private static void CopyGrayRowToBgra(byte[] sourceRow, byte[] targetRow, int width)
		{
			for (int x = 0; x < width; x++)
			{
				byte value = sourceRow[x];
				int target = x * 4;
				targetRow[target] = value;
				targetRow[target + 1] = value;
				targetRow[target + 2] = value;
				targetRow[target + 3] = 255;
			}
		}

		private static void CopyPlanarColorRowToBgra(
			byte[] redRow,
			byte[] greenRow,
			byte[] blueRow,
			byte[] targetRow,
			int width)
		{
			for (int x = 0; x < width; x++)
			{
				int target = x * 4;
				targetRow[target] = blueRow[x];
				targetRow[target + 1] = greenRow[x];
				targetRow[target + 2] = redRow[x];
				targetRow[target + 3] = 255;
			}
		}
	}
}
