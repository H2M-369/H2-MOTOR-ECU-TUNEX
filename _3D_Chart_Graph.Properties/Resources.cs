using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace _3D_Chart_Graph.Properties;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class Resources
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				ResourceManager resourceManager = new ResourceManager("_3D_Chart_Graph.Properties.Resources", typeof(Resources).Assembly);
				resourceMan = resourceManager;
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	internal static Bitmap Analis
	{
		get
		{
			object obj = ResourceManager.GetObject("Analis", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap WhatsApp_Image_2025_06_28_at_09_35_19
	{
		get
		{
			object obj = ResourceManager.GetObject("WhatsApp Image 2025-06-28 at 09.35.19", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap WhatsApp_Image_2025_06_28_at_12_52_53
	{
		get
		{
			object obj = ResourceManager.GetObject("WhatsApp Image 2025-06-28 at 12.52.53", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap WhatsApp_Image_2025_06_28_at_16_09_29
	{
		get
		{
			object obj = ResourceManager.GetObject("WhatsApp Image 2025-06-28 at 16.09.29", resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal Resources()
	{
	}
}
