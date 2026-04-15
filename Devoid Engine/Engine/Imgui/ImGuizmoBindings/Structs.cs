using System.Numerics;
using System.Runtime.InteropServices;

namespace DevoidEngine.Engine.Imgui.ImGuizmoBindings
{
	[StructLayout(LayoutKind.Sequential)]
	public struct ImGuizmoStyle
	{
		public float TranslationLineThickness;
		public float TranslationLineArrowSize;
		public float RotationLineThickness;
		public float RotationOuterLineThickness;
		public float ScaleLineThickness;
		public float ScaleLineCircleSize;
		public float HatchedAxisLineThickness;
		public float CenterCircleSize;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)Color.Count)]
		public Vector4[] Colors;
	}
}