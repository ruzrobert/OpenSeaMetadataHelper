using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace OpenSeaMetadataHelper
{
	/// <summary>
	/// https://docs.opensea.io/docs/metadata-standards
	/// </summary>
	public class Metadata
	{
		/// <summary>
		/// Name of the item.
		/// </summary>
		[JsonProperty("name")]
		public string Name { get; set; }

		/// <summary>
		/// A human readable description of the item. Markdown is supported.
		/// </summary>
		[JsonProperty("description")]
		public string Description { get; set; }

		/// <summary>
		/// This is the URL to the image of the item. Can be just about any type of image (including SVGs, which will be cached into PNGs by OpenSea), and can be IPFS URLs or paths. We recommend using a 350 x 350 image.
		/// </summary>
		[JsonProperty("image")]
		public string Image { get; set; }

		/// <summary>
		/// Raw SVG image data, if you want to generate images on the fly (not recommended). Only use this if you're not including the image parameter.
		/// </summary>
		[JsonProperty("image_data")]
		public string ImageData { get; set; }

		/// <summary>
		/// This is the URL that will appear below the asset's image on OpenSea and will allow users to leave OpenSea and view the item on your site.
		/// </summary>
		[JsonProperty("external_url")]
		public string ExternalUrl { get; set; }

		/// <summary>
		/// Background color of the item on OpenSea. Must be a six-character hexadouble without a pre-pended #.
		/// </summary>
		[JsonProperty("background_color")]
		public string BackgroundColor { get; set; }

		/// <summary>
		/// A URL to a multi-media attachment for the item. The file extensions GLTF, GLB, WEBM, MP4, M4V, OGV, and OGG are supported, along with the audio-only extensions MP3, WAV, and OGA.
		/// 
		/// Animation_url also supports HTML pages, allowing you to build rich experiences and interactive NFTs using JavaScript canvas, WebGL, and more.Scripts and relative paths within the HTML page are now supported. However, access to browser extensions is not supported.
		/// </summary>
		[JsonProperty("animation_url")]
		public string AnimationUrl { get; set; }

		/// <summary>
		/// A URL to a YouTube video.
		/// </summary>
		[JsonProperty("youtube_url")]
		public string YoutubeUrl { get; set; }

		/// <summary>
		/// These are the attributes for the item, which will show up on the OpenSea page for the item. (see below)
		/// </summary>
		[JsonProperty("attributes")]
		public List<MdAttribute> Attributes { get; set; }
		
		public void Save(string path)
		{
			string json = Serialize();
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			File.WriteAllText(path, json, Encoding.UTF8);
		}

		public string Serialize()
		{
			OnBeforeSerializing();
			
			JsonSerializer jsonSerializer = JsonSerializer.CreateDefault(null);
			jsonSerializer.NullValueHandling = NullValueHandling.Ignore;
			jsonSerializer.Converters.Add(new DoubleJsonConverter());

			StringWriter sw = new StringWriter(new StringBuilder(), CultureInfo.InvariantCulture);
			using (JsonTextWriter jsonWriter = new JsonTextWriter(sw))
			{
				jsonWriter.Formatting = Formatting.Indented;
				jsonWriter.IndentChar = '\t';
				jsonWriter.Indentation = 1;

				jsonSerializer.Serialize(jsonWriter, this);
			}

			return sw.ToString();
		}

		private void OnBeforeSerializing()
		{
			if (ImageData != null) Image = null;
			Require(BackgroundColor == null || (!BackgroundColor.StartsWith("#") && BackgroundColor.Length == 6));
		}

		private static void Require(bool check)
		{
			if (!check) throw new InvalidOperationException();
		}
	}

	public class MdAttribute
	{
		/// <summary>
		/// 
		/// </summary>
		[JsonProperty("display_type")]
		public string DisplayType { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[JsonProperty("trait_type")]
		public string TraitType { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[JsonProperty("value", NullValueHandling = NullValueHandling.Include)]
		public object Value { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[JsonProperty("max_value")]
		public object MaxValue { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[JsonIgnore]
		public DateTime DateValue
		{
			set
			{
				DisplayType = SerializableDisplayTypes.FromDisplayType(OpenSeaMetadataHelper.DisplayType.Date);
				Value = ((DateTimeOffset)value).ToUnixTimeSeconds();
			}
		}

		public MdAttribute(DisplayType displayType, string traitType, string value) : this(displayType, traitType, value, null) { }
		public MdAttribute(DisplayType displayType, string traitType, double value) : this(displayType, traitType, value, null) { }
		public MdAttribute(DisplayType displayType, string traitType, double value, double maxValue)
											: this(displayType, traitType, (object)value, maxValue) { }

		private MdAttribute(DisplayType displayType, string traitType, object value, double? maxValue)
		{
			DisplayType = SerializableDisplayTypes.FromDisplayType(displayType);
			TraitType = traitType;
			Value = value;
			MaxValue = maxValue.HasValue ? (object)maxValue.Value : null;
		}

		public MdAttribute(string traitType, DateTime date)
		{
			TraitType = traitType;
			DateValue = date;
		}

		public MdAttribute(string traitType, string value)
		{
			TraitType = traitType;
			Value = value;
		}

		public MdAttribute(string traitType, double value)
		{
			TraitType = traitType;
			Value = value;
		}

		public MdAttribute(string traitType, double value, double maxValue)
		{
			TraitType = traitType;
			Value = value;
			MaxValue = maxValue;
		}

		public MdAttribute(string value) => Value = value;
		public MdAttribute(double value) => Value = value;

		private static class SerializableDisplayTypes
		{
			public const string None = null;
			public const string Number = "number";
			public const string BoostPercentage = "boost_percentage";
			public const string BoostNumber = "boost_number";
			public const string Date = "date";

			public static string[] GetValues() => new[] { None, Number, BoostPercentage, BoostNumber, Date };

			public static string FromDisplayType(DisplayType displayType)
			{
				return GetValues()[(int)displayType];
			}
		}
	}

	public enum DisplayType
	{
		None = 0,
		Number = 1,
		BoostPercentage = 2,
		BoostNumber = 3,
		Date = 4,
	}

	/// <summary>
	/// https://stackoverflow.com/questions/21153381/json-net-serializing-float-double-with-minimal-decimal-places-i-e-no-redundant
	/// https://stackoverflow.com/questions/65093530/prevent-newtonsoft-json-from-adding-trailing-0
	/// </summary>
	public class DoubleJsonConverter : JsonConverter<double>
	{
		public override bool CanRead => false;

		public override double ReadJson(JsonReader reader, Type objectType, double existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
		}

		public override void WriteJson(JsonWriter writer, double value, JsonSerializer serializer)
		{
			if (IsWholeValue(value))
			{
				writer.WriteRawValue(JsonConvert.ToString(Convert.ToInt64(value)));
			}
			else
			{
				writer.WriteRawValue(JsonConvert.ToString(value));
			}
		}

		private static bool IsWholeValue(object value)
		{
			if (value is decimal decimalValue)
			{
				int precision = (decimal.GetBits(decimalValue)[3] >> 16) & 0x000000FF;
				return precision == 0;
			}
			else if (value is float floatValue)
			{
				return floatValue == Math.Truncate(floatValue);
			}
			else if (value is double doubleValue)
			{
				return doubleValue == Math.Truncate(doubleValue);
			}

			return false;
		}
	}
}