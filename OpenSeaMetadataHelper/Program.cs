using System;
using System.Collections.Generic;
using System.IO;
using static System.Environment;

namespace OpenSeaMetadataHelper
{
	internal class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("OpenSea Metadata Helper");
			Console.WriteLine();

			RunHelper();

			Console.WriteLine("Done!!!");
			Console.ReadKey();
		}

		private static void RunHelper()
		{
			// Examples

			Metadata metadata = new Metadata()
			{
				Name = "Dave Starbelly",
				Description = "Friendly OpenSea Creature that enjoys long swims in the ocean.",
				Image = "https://storage.googleapis.com/opensea-prod.appspot.com/puffs/3.png",
				ExternalUrl = "https://openseacreatures.io/3",
				Attributes = new List<MdAttribute>()
				{
					new MdAttribute("Base", "Starfish"),
					new MdAttribute("Eyes", "Big"),
					new MdAttribute("Mouth", "Surprised"),
					new MdAttribute("Level", 5),
					new MdAttribute("Stamina", 1.4),
					new MdAttribute("Personality", "Sad"),
					new MdAttribute(DisplayType.BoostNumber, "Aqua Power", 40),
					new MdAttribute(DisplayType.BoostPercentage, "Stamina Increase", 10),
					new MdAttribute(DisplayType.Number, "Generation", 2),
					new MdAttribute(DisplayType.Date, "Birthday1", 1546360800),
					new MdAttribute("Birthday2", new DateTime(2019, 1, 1, 18, 40, 0)),
					new MdAttribute("Happy"),
					new MdAttribute("MaxValued", 10, 100),
				},
			};

			metadata.Save(Path.Combine(GetFolderPath(SpecialFolder.DesktopDirectory), "metadata_0.json"));
		}
	}
}
