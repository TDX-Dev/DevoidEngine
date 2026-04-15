using System.Numerics;
using System.Runtime.InteropServices;

namespace DevoidEngine.Engine.Imgui.ImGuizmoBindings;

internal static class Native
{
	private const string LibraryName = "cimguizmo.dll";

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ImGuizmo_SetDrawlist(IntPtr drawlist);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ImGuizmo_BeginFrame();

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ImGuizmo_SetImGuiContext(IntPtr ctx);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool ImGuizmo_IsOver_Nil();

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool ImGuizmo_IsUsing();

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool ImGuizmo_IsUsingViewManipulate();

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool ImGuizmo_IsViewManipulateHovered();

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool ImGuizmo_IsUsingAny();

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ImGuizmo_Enable(bool enable);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ImGuizmo_DecomposeMatrixToComponents(float[] matrix, float[] translation, float[] rotation, float[] scale);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ImGuizmo_RecomposeMatrixFromComponents(float[] translation, float[] rotation, float[] scale, float[] matrix);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ImGuizmo_SetRect(float x, float y, float width, float height);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ImGuizmo_SetOrthographic(bool isOrthographic);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ImGuizmo_DrawCubes(float[] view, float[] projection, float[] matrices, int matrixCount);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ImGuizmo_DrawGrid(float[] view, float[] projection, float[] matrix, float gridSize);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool ImGuizmo_Manipulate(float[] view, float[] projection, Operation operation, Mode mode, float[] matrix, float[] deltaMatrix, float[]? snap, float[]? localBounds, float[]? boundsSnap);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ImGuizmo_ViewManipulate_Float(float[] view, float length, Vector2 position, Vector2 size, uint backgroundColor);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ImGuizmo_ViewManipulate_FloatPtr(float[] view, float[] projection, Operation operation, Mode mode, float[] matrix, float length, Vector2 position, Vector2 size, uint backgroundColor);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ImGuizmo_SetAlternativeWindow(IntPtr window);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ImGuizmo_SetID(int id);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ImGuizmo_PushID_Str(string str_id);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ImGuizmo_PushID_StrStr(string str_id_begin, string str_id_end);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ImGuizmo_PushID_Ptr(IntPtr ptr_id);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ImGuizmo_PushID_Int(int int_id);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ImGuizmo_PopID();

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint ImGuizmo_GetID_Str(string str_id);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint ImGuizmo_GetID_StrStr(string str_id_begin, string str_id_end);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint ImGuizmo_GetID_Ptr(IntPtr ptr_id);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool ImGuizmo_IsOver_OPERATION(Operation op);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ImGuizmo_SetGizmoSizeClipSpace(float value);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ImGuizmo_AllowAxisFlip(bool value);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ImGuizmo_SetAxisLimit(float value);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ImGuizmo_SetAxisMask(bool x, bool y, bool z);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ImGuizmo_SetPlaneLimit(float value);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool ImGuizmo_IsOver_FloatPtr(float[] position, float pixelRadius);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr Style_Style();

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void Style_destroy(IntPtr self);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr ImGuizmo_GetStyle();
}