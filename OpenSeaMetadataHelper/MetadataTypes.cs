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
			jsonSerializer.Converters.Add(new DisplayTypeJsonConverter());

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
		[JsonProperty("display_type", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public DisplayType DisplayType { get; set; }

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
		public double? MaxValue { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[JsonIgnore]
		public DateTime DateValue
		{
			set
			{
				DisplayType = DisplayType.Date;
				Value = ((DateTimeOffset)value).ToUnixTimeSeconds();
			}
		}

		public MdAttribute(DisplayType displayType, string traitType, string value) : this(displayType, traitType, value, null) { }
		public MdAttribute(DisplayType displayType, string traitType, double value) : this(displayType, traitType, value, null) { }
		public MdAttribute(DisplayType displayType, string traitType, double value, double maxValue)
											: this(displayType, traitType, (object)value, maxValue) { }

		private MdAttribute(DisplayType displayType, string traitType, object value, double? maxValue)
		{
			DisplayType = displayType;
			TraitType = traitType;
			Value = value;
			MaxValue = maxValue ?? default;
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
	public class DoubleJsonConverter : JsonConverter
	{
		public override bool CanRead => false;

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			double doubleValue = (double)value;

			if (IsWholeValue(doubleValue))
			{
				writer.WriteRawValue(JsonConvert.ToString(Convert.ToInt64(value)));
			}
			else
			{
				writer.WriteRawValue(JsonConvert.ToString(value));
			}
		}

		private static bool IsWholeValue(double value)
		{
			return value == Math.Truncate(value);
		}

		public override bool CanConvert(Type objectType)
		{
			if (objectType == typeof(double)) return true;
			if (objectType != null && objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Nullable<>)
					&& objectType.GetGenericArguments()[0] == typeof(double)) return true;
			return false;
		}
	}

	public class DisplayTypeJsonConverter : JsonConverter<DisplayType>
	{
		public override DisplayType ReadJson(JsonReader reader, Type objectType, DisplayType existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			if (reader.Value is string strValue)
			{
				switch (strValue)
				{
					case "number": return DisplayType.Number;
					case "boost_percentage": return DisplayType.BoostPercentage;
					case "boost_number": return DisplayType.BoostNumber;
					case "date": return DisplayType.Date;
				}
			}

			return DisplayType.None;
		}

		public override void WriteJson(JsonWriter writer, DisplayType value, JsonSerializer serializer)
		{
			switch (value)
			{
				case DisplayType.None:
					writer.WriteNull(); break;
				case DisplayType.Number:
					writer.WriteValue("number"); break;
				case DisplayType.BoostPercentage:
					writer.WriteValue("boost_percentage"); break;
				case DisplayType.BoostNumber:
					writer.WriteValue("boost_number"); break;
				case DisplayType.Date:
					writer.WriteValue("date"); break;
				default:
					writer.WriteNull(); break;
			}
		}
	}
}