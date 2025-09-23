using System.Globalization;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using E_Commerce.Data;
using E_Commerce.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Utils
{
    public class DataSeeder
    {
        public static async Task SeedCategoriesData(ApplicationDbContext context, string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Categories file does not exist.");
                return;
            }

            var existingCategoryNames = await context.Categories.Select(c => c.Name.ToLower()).ToHashSetAsync();
            var categoriesToAdd = new List<Category>();

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true }))
            {
                var records = csv.GetRecords<Category>().ToList();

                foreach (var record in records)
                {
                    if (!existingCategoryNames.Contains(record.Name.ToLower()))
                    {
                        categoriesToAdd.Add(new Category { Name = record.Name });
                    }
                }
            }

            if (categoriesToAdd.Any())
            {
                context.Categories.AddRange(categoriesToAdd);
                await context.SaveChangesAsync();
            }
        }


        public static async Task SeedSubCategoriesData(ApplicationDbContext context, string filePath)
        {

            if (!File.Exists(filePath))
            {
                Console.WriteLine("File does not exists!!!");
                return;
            }

            var existingSubCategories = await context.SubCategories.Select(sc => sc.Name.ToLower()).ToHashSetAsync();
            var subCategoriesToAdd = new List<SubCategory>();

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true }))
            {
                var records = csv.GetRecords<SubCategory>().ToList();

                foreach (var record in records)
                {
                    if (!existingSubCategories.Contains(record.Name.ToLower()))
                    {
                        subCategoriesToAdd.Add(new SubCategory { Name = record.Name, CategoryId = record.CategoryId });
                    }
                }
            }

            if (subCategoriesToAdd.Any())
            {
                context.SubCategories.AddRange(subCategoriesToAdd);
                await context.SaveChangesAsync();
            }

        }
    }
}
