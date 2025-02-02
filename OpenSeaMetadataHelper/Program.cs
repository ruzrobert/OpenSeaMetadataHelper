using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static System.Environment;

namespace OpenSeaMetadataHelper
{
	internal class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("OpenSea Metadata Helper");
			Console.WriteLine();

			do
			{
				Console.WriteLine("Press Enter to run");
			}
			while (Console.ReadKey().Key != ConsoleKey.Enter);

			//TestHelper();
			//GeneratePaintingsMetadata();
			GenerateRandomizedImageIdsForPaintings();

			Console.WriteLine("Done!!!");
			Console.ReadKey();
		}

		private static void GenerateRandomizedImageIdsForPaintings()
		{
			Random random = new Random(1000);
			List<int> imageIds = Enumerable.Range(1, 80).OrderBy(x => random.Next()).ToList();
			List<string> imageIdsStr = imageIds.Select(x => $"{x:000}.png").ToList();
		}

		private static void GeneratePaintingsMetadata()
		{
			const string ZeroDescription = "My friend, Sergey Vudu, is a big fan of the great artist, Van Gogh. He asked me to make a copy of Van Gogh's painting. I agreed and offered to create it in my style with my own technique which is in the form of volumetric pixels. The final painting would possess impressionism with textured volume when looking up close. But when you step away, the painting looks like the great artist's original painting, with large pixels in the foreground and with classical strokes in the background as painted by Van Gogh. Acrylic paint was used in the art work, making it different from the original, which was created with oils.   The cons of using oil paints are that they dry longer, meaning the flow of pixels would have taken lots of time and that wouldn’t be convenient. On the other hand, using acrylic sped up the process but the downside is that acrylic changes shades and saturation in comparison to oil paints, making it hard to match the transitions of strokes in the original piece. In the end, powerful energy was radiating from the painting. I am sincerely happy that I have been immersed into the creativity of the great artist of the past era. Thanks to a friend, I created a marvellous masterpiece.";
			const string PieceDescription = "7.25 x 7.25 cm piece of Being Van Gohg 58 x 73 cm masterpiece. Digitized real world art. The collection is inspired by the work of great masters of the past and present. Admiring the talented hands of genius masters, the artist does not only copy their masterpieces but uses his own inherent technique that creates a new interpretation of already well-known paintings and at times, turning these paintings into completely different visual images. These images may portray a whole different meaning at the heart of contemplating people, but also give a different feeling from owning such works of art.";

			string saveDirectory = @"G:\Мой диск\Projects\Smart Contracts   NFT\Digital Factory\Painting Digitization\Metadata";
			string website = "https://artkrys.gallery/";
			string ipfsBaseUri = "ipfs://bafybeihhzutah5kwvhbqodzknbi75issosgsyw4bilmq6rnhbmigeg75pm/";

			Metadata mainMetadata = new Metadata()
			{
				Name = "BEING VAN GOGH",
				Description = ZeroDescription,
				ExternalUrl = website,
				Image = $"{ipfsBaseUri}000.png",
				Attributes = new List<MdAttribute>()
				{
					new MdAttribute("Real-World Size", "58 x 73 cm"),
					new MdAttribute("Paint Type", "Acrylic"),
					new MdAttribute("Technique", "Volumetric Pixels"),
					new MdAttribute("Type", "Whole Painting"),
					new MdAttribute("Unique", "1 of 1"),
					new MdAttribute("Artwork Completed", new DateTime(2022, 4, 17)),
				},
			};
			mainMetadata.Save(Path.Combine(saveDirectory, "0"));

			Random random = new Random(1000);
			List<int> imageIds = Enumerable.Range(1, 80).OrderBy(x => random.Next()).ToList();

			for (int i = 1; i <= 80; i++)
			{
				int imageId = imageIds[i - 1];
				const int COLUMNS = 8;
				const int ROWS = 10;

				Metadata metadata = new Metadata()
				{
					Name = $"#{i:00}",
					Description = PieceDescription,
					ExternalUrl = website,
					Image = $"{ipfsBaseUri}{imageId:000}.png",
					Attributes = new List<MdAttribute>()
					{
						new MdAttribute("Real-World Size", "7.25 x 7.25 cm"),
						new MdAttribute("Paint Type", "Acrylic"),
						new MdAttribute("Technique", "Volumetric Pixels"),
						new MdAttribute("Piece Location", $"X: {((imageId - 1) % COLUMNS) + 1}, Y: {ROWS - ((imageId - 1) / COLUMNS)}"),
						new MdAttribute("Type", "Painting Piece"),
						new MdAttribute("Artwork Completed", new DateTime(2022, 4, 17)),
					},
				};
				metadata.Save(Path.Combine(saveDirectory, i.ToString()));
			}

			Process.Start(saveDirectory);
		}

		private static void TestHelper()
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