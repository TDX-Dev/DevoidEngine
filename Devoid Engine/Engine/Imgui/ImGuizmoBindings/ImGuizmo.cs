using System.Numerics;
using System.Runtime.InteropServices;

namespace DevoidEngine.Engine.Imgui.ImGuizmoBindings;

public class ImGuizmo
{
	private static readonly float[] matrixBuffer = new float[16];

	public static void SetDrawlist(IntPtr drawlist)
	{
		Native.ImGuizmo_SetDrawlist(drawlist);
	}

	public static void BeginFrame()
	{
		Native.ImGuizmo_BeginFrame();
	}

	public static void SetImGuiContext(IntPtr ctx)
	{
		Native.ImGuizmo_SetImGuiContext(ctx);
	}

	public static bool IsOver()
	{
		return Native.ImGuizmo_IsOver_Nil();
	}

	public static bool IsUsing()
	{
		return Native.ImGuizmo_IsUsing();
	}

	public static bool IsUsingViewManipulate()
	{
		return Native.ImGuizmo_IsUsingViewManipulate();
	}

	public static bool IsViewManipulateHovered()
	{
		return Native.ImGuizmo_IsViewManipulateHovered();
	}

	public static bool IsUsingAny()
	{
		return Native.ImGuizmo_IsUsingAny();
	}

	public static void Enable(bool enable)
	{
		Native.ImGuizmo_Enable(enable);
	}

	public static void DecomposeMatrixToComponents(Matrix4x4 matrix, out Vector3 translation, out Vector3 rotation, out Vector3 scale)
	{
		MatrixToArray(matrix, matrixBuffer);
		float[] translationArray = new float[3];
		float[] rotationArray = new float[3];
		float[] scaleArray = new float[3];
		Native.ImGuizmo_DecomposeMatrixToComponents(matrixBuffer, translationArray, rotationArray, scaleArray);
		translation = new Vector3(translationArray[0], translationArray[1], translationArray[2]);
		rotation = new Vector3(rotationArray[0], rotationArray[1], rotationArray[2]);
		scale = new Vector3(scaleArray[0], scaleArray[1], scaleArray[2]);
	}

	public static Matrix4x4 RecomposeMatrixFromComponents(Vector3 translation, Vector3 rotation, Vector3 scale)
	{
		float[] translationArray = new[] { translation.X, translation.Y, translation.Z };
		float[] rotationArray = new[] { rotation.X, rotation.Y, rotation.Z };
		float[] scaleArray = new[] { scale.X, scale.Y, scale.Z };
		Native.ImGuizmo_RecomposeMatrixFromComponents(translationArray, rotationArray, scaleArray, matrixBuffer);
		return ArrayToMatrix(matrixBuffer);
	}

	public static void SetRect(float x, float y, float width, float height)
	{
		Native.ImGuizmo_SetRect(x, y, width, height);
	}

	public static void SetOrthographic(bool isOrthographic)
	{
		Native.ImGuizmo_SetOrthographic(isOrthographic);
	}

	public static void DrawCubes(Matrix4x4 view, Matrix4x4 projection, Matrix4x4[] matrices)
	{
		float[] viewArray = new float[16];
		MatrixToArray(view, viewArray);

		float[] projectionArray = new float[16];
		MatrixToArray(projection, projectionArray);

		float[] matricesArray = new float[matrices.Length * 16];
		for (int i = 0; i < matrices.Length; i++)
		{
			MatrixToArray(matrices[i], matricesArray, i * 16);
		}

		Native.ImGuizmo_DrawCubes(viewArray, projectionArray, matricesArray, matrices.Length);
	}

	public static void DrawGrid(Matrix4x4 view, Matrix4x4 projection, Matrix4x4 matrix, float gridSize)
	{
		float[] viewArray = new float[16];
		MatrixToArray(view, viewArray);

		float[] projectionArray = new float[16];
		MatrixToArray(projection, projectionArray);

		float[] matrixArray = new float[16];
		MatrixToArray(matrix, matrixArray);

		Native.ImGuizmo_DrawGrid(viewArray, projectionArray, matrixArray, gridSize);
	}

	public static bool Manipulate(
		Matrix4x4 view,
		Matrix4x4 projection,
		Operation operation,
		Mode mode,
		ref Matrix4x4 matrix,
		out Matrix4x4 deltaMatrix,
		Vector3? snap = null,
		float[]? localBounds = null,
		float[]? boundsSnap = null)
	{
		float[] viewArray = new float[16];
		MatrixToArray(view, viewArray);

		float[] projectionArray = new float[16];
		MatrixToArray(projection, projectionArray);

		float[] matrixArray = new float[16];
		MatrixToArray(matrix, matrixArray);

		float[] deltaMatrixArray = new float[16];
		float[]? snapArray = snap.HasValue ? new[] { snap.Value.X, snap.Value.Y, snap.Value.Z } : null;

		bool result = Native.ImGuizmo_Manipulate(viewArray, projectionArray, operation, mode, matrixArray, deltaMatrixArray, snapArray, localBounds, boundsSnap);

		matrix = ArrayToMatrix(matrixArray);
		deltaMatrix = ArrayToMatrix(deltaMatrixArray);

		return result;
	}

	public static void ViewManipulate(ref Matrix4x4 view, float length, Vector2 position, Vector2 size, uint backgroundColor)
	{
		float[] viewArray = new float[16];
		MatrixToArray(view, viewArray);
		Native.ImGuizmo_ViewManipulate_Float(viewArray, length, position, size, backgroundColor);
		view = ArrayToMatrix(viewArray);
	}

	public static void ViewManipulate(ref Matrix4x4 view, ref Matrix4x4 projection, Operation operation, Mode mode, ref Matrix4x4 matrix, float length, Vector2 position, Vector2 size, uint backgroundColor)
	{
		float[] viewArray = new float[16];
		MatrixToArray(view, viewArray);

		float[] projectionArray = new float[16];
		MatrixToArray(projection, projectionArray);

		float[] matrixArray = new float[16];
		MatrixToArray(matrix, matrixArray);

		Native.ImGuizmo_ViewManipulate_FloatPtr(viewArray, projectionArray, operation, mode, matrixArray, length, position, size, backgroundColor);

		view = ArrayToMatrix(viewArray);
		projection = ArrayToMatrix(projectionArray);
		matrix = ArrayToMatrix(matrixArray);
	}

	public static void SetAlternativeWindow(IntPtr window)
	{
		Native.ImGuizmo_SetAlternativeWindow(window);
	}

	public static void SetId(int id)
	{
		Native.ImGuizmo_SetID(id);
	}

	public static void PushId(string str_id)
	{
		Native.ImGuizmo_PushID_Str(str_id);
	}

	public static void PushId(string str_id_begin, string str_id_end)
	{
		Native.ImGuizmo_PushID_StrStr(str_id_begin, str_id_end);
	}

	public static void PushId(IntPtr ptr_id)
	{
		Native.ImGuizmo_PushID_Ptr(ptr_id);
	}

	public static void PushId(int int_id)
	{
		Native.ImGuizmo_PushID_Int(int_id);
	}

	public static void PopId()
	{
		Native.ImGuizmo_PopID();
	}

	public static uint GetId(string str_id)
	{
		return Native.ImGuizmo_GetID_Str(str_id);
	}

	public static uint GetId(string str_id_begin, string str_id_end)
	{
		return Native.ImGuizmo_GetID_StrStr(str_id_begin, str_id_end);
	}

	public static uint GetId(IntPtr ptr_id)
	{
		return Native.ImGuizmo_GetID_Ptr(ptr_id);
	}

	public static bool IsOver(Operation op)
	{
		return Native.ImGuizmo_IsOver_OPERATION(op);
	}

	public static void SetGizmoSizeClipSpace(float value)
	{
		Native.ImGuizmo_SetGizmoSizeClipSpace(value);
	}

	public static void AllowAxisFlip(bool value)
	{
		Native.ImGuizmo_AllowAxisFlip(value);
	}

	public static void SetAxisLimit(float value)
	{
		Native.ImGuizmo_SetAxisLimit(value);
	}

	public static void SetAxisMask(bool x, bool y, bool z)
	{
		Native.ImGuizmo_SetAxisMask(x, y, z);
	}

	public static void SetPlaneLimit(float value)
	{
		Native.ImGuizmo_SetPlaneLimit(value);
	}

	public static bool IsOver(Vector3 position, float pixelRadius)
	{
		float[] pos = new[] { position.X, position.Y, position.Z };
		return Native.ImGuizmo_IsOver_FloatPtr(pos, pixelRadius);
	}

	public static ImGuizmoStyle GetStyle()
	{
		IntPtr nativeStyle = Native.ImGuizmo_GetStyle();
		return Marshal.PtrToStructure<ImGuizmoStyle>(nativeStyle);
	}

	public static void SetStyle(ImGuizmoStyle style)
	{
		IntPtr nativeStyle = Native.ImGuizmo_GetStyle();
		// Overwrite the native style memory with the managed struct values.
		// false tells the runtime not to attempt to free any existing managed data.
		Marshal.StructureToPtr(style, nativeStyle, false);
	}

	private static void MatrixToArray(Matrix4x4 matrix, float[] array, int offset = 0)
	{
		array[offset + 0] = matrix.M11;
		array[offset + 1] = matrix.M12;
		array[offset + 2] = matrix.M13;
		array[offset + 3] = matrix.M14;
		array[offset + 4] = matrix.M21;
		array[offset + 5] = matrix.M22;
		array[offset + 6] = matrix.M23;
		array[offset + 7] = matrix.M24;
		array[offset + 8] = matrix.M31;
		array[offset + 9] = matrix.M32;
		array[offset + 10] = matrix.M33;
		array[offset + 11] = matrix.M34;
		array[offset + 12] = matrix.M41;
		array[offset + 13] = matrix.M42;
		array[offset + 14] = matrix.M43;
		array[offset + 15] = matrix.M44;
	}

	private static Matrix4x4 ArrayToMatrix(float[] array, int offset = 0)
	{
		return new Matrix4x4(
			array[offset + 0], array[offset + 1], array[offset + 2], array[offset + 3],
			array[offset + 4], array[offset + 5], array[offset + 6], array[offset + 7],
			array[offset + 8], array[offset + 9], array[offset + 10], array[offset + 11],
			array[offset + 12], array[offset + 13], array[offset + 14], array[offset + 15]
		);
	}
}