using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using ImporterBLL.Helpers;
using ImporterBLL.Objects;
using WoolworthsDAL;

namespace ImporterBLL.Importers
{
    public class RecipePEL : ServiceImporterBase
    {
        private readonly List<string> _fileNames = new List<string>();
        private readonly List<string> _featuredFileNames = new List<string>();
        private readonly string _imgDir;
        private readonly string _imgPathURL;
        

        public RecipePEL(string fileDirectoryPath, string archiveDirectoryPath, string stagingTableName, string formatFilePath, IEnumerable<string> fileNames,
            IEnumerable<string> featuredFileNames, string summaryReportErrorToEmailAddress, string summaryReportFromEmailAddress, string summaryReportFromAddressFriendlyName, 
            string imageDir, string imgPathURL,
            string sqlaServerPath, string sqlbServerPath, string localSqlPath, string tempUploadFolder, string daysToRun)
            : base(ServiceLogger.FlatFileType.RecipePEL, FlatFiles.FileFormat.JSON, FlatFiles.CompressType.None,
            fileDirectoryPath, archiveDirectoryPath, stagingTableName, formatFilePath, summaryReportErrorToEmailAddress, summaryReportFromEmailAddress, summaryReportFromAddressFriendlyName,
            sqlaServerPath, sqlbServerPath, localSqlPath, tempUploadFolder, daysToRun, false)
        {
            _imgDir = imageDir;
            _imgPathURL = imgPathURL;

            foreach (var fileName in fileNames)
            {
                _fileNames.Add(fileName);
            }
            foreach (var featured in featuredFileNames)
            {
                _featuredFileNames.Add(featured);
            }
        }


        // file to process
        protected override List<string> FileNames
        {
            get
            {
                return _fileNames;
            }
        }

        // directory to move images to
        public string ImgDir
        {
            get
            {
                return _imgDir;
            }
        }

        // absolute path to the folder where images are served from over the interweb
        public string ImgPathURL
        {
            get
            {
                return _imgPathURL;
            }
        }

        protected override void ProcessBespoke()
        {
            Log(LogType.Log, String.Format("Executing bespoke import on path {0}", FileDirectoryPath));

            //todo: where to log errors in the process
            // de-serialise the JSON here and process recipes, one by one
            var js = new JavaScriptSerializer();
            
            js.MaxJsonLength = 2147483644;
            string resp;
            foreach (var path in UnzippedPaths)
            {
                using (var jsonStream = FlatFiles.GetS3File(path))
                {

                    using (var reader = new StreamReader(jsonStream, Encoding.Default)) // be careful with this, the "default" is the windows system setting, and should be fine in this context.
                    {
                        var temp = reader.ReadToEnd();
                        resp = Encoding.UTF8.GetString(Encoding.Convert(reader.CurrentEncoding, Encoding.UTF8, reader.CurrentEncoding.GetBytes(temp)));
                        reader.Close();
                    }
                }

                    var list = js.Deserialize<RecipeList>(resp);

                    // only action if there are recipes 
                    if (list.recipes != null)
                    {
                        // insert/update/delete Recipe record
                        foreach (var recipe in list.recipes)
                        {
                            switch (recipe.Action.ToLower())
                            {
                                case "d":
                                    deleteRecipe(recipe.RecipeID);
                                    break;
                                case "i":
                                case "u":
                                    updateRecipe(recipe);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
            }

            foreach (var featuredFileName in _featuredFileNames)
            {
                var featuredPath = String.Format("{0}{1}", FileDirectoryPath, featuredFileName);
                if (File.Exists(featuredPath))
                {
                    string data;
                    using (var reader = new StreamReader(featuredPath))
                    {
                        data = reader.ReadToEnd();
                        reader.Close();
                    }

                    var featuredList = (FeaturedRecipeList)js.Deserialize(data, typeof(FeaturedRecipeList));
                    foreach (var featuredRecipe in featuredList.FeaturedRecipe)
                    {
                        updateRecipeAsFeatured(featuredRecipe);
                    }

                    FlatFiles.Archive(featuredPath, ArchiveDirectoryPath, true, FileFormat, TempUploadFolder);
                }
            }
            Log(LogType.Log, String.Format("Finished bespoke import on path {0}", FileDirectoryPath));
        }

        // adds or returns the actual Recipe record (id)
        private int createRecipeRecord(RecipeObj recipe)
        {
            using (var context = new WoolworthsDBDataContext())
            {
                var existing = context.Recipes.SingleOrDefault(r => r.ID == recipe.RecipeID);

                // if the recipe already exists and the action isn't to update then just return the recipe identifier
                if (existing != null) //Always update, regardless of action (well, it should be "i" or "u" -- && recipe.Action.ToLower() == "u")
                {
                    // if the recipe exists and the instruction is to update then updates
                    existing.CookingTime = recipe.CookingTime;
                    existing.Description = recipe.Description;
                    existing.Recipe1 = recipe.Recipe;
                    existing.ImageURL = downloadRemoteImageFile(recipe.ImageURL, ImgDir, recipe.Recipe);
                    existing.OriginImageUrl = recipe.ImageURL;
                    existing.PrepTime = recipe.PreparationTime;
                    existing.Serves = recipe.Serves;
                    existing.Tip = recipe.Tip;
                    existing.Title = recipe.Title;
                    existing.TotalTime = recipe.TotalTime;
                    context.SubmitChanges();
                    return existing.ID;
                }
                else 
                {
                    // create the record regardless of whether it was an insert or update record
                    var newRecipe = new WoolworthsDAL.Recipe();
                    newRecipe.ID = recipe.RecipeID;
                    newRecipe.CookingTime = recipe.CookingTime;
                    newRecipe.Recipe1 = recipe.Recipe;
                    newRecipe.Description = recipe.Description;
                    newRecipe.ImageURL = downloadRemoteImageFile(recipe.ImageURL, ImgDir, recipe.Recipe);
                    newRecipe.OriginImageUrl = recipe.ImageURL;
                    newRecipe.PrepTime = recipe.PreparationTime;
                    newRecipe.Serves = recipe.Serves;
                    newRecipe.Tip = recipe.Tip;
                    newRecipe.Title = recipe.Title;
                    newRecipe.TotalTime = recipe.TotalTime;
                    context.Recipes.InsertOnSubmit(newRecipe);
                    context.SubmitChanges();
                    return newRecipe.ID;
                }
            }
        }

        // downloads an image from a given URL and stores it on the file system
        private string downloadRemoteImageFile(string uri, string imgDir, string title)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(uri);
                var response = (HttpWebResponse)request.GetResponse();

                // Check that the remote file was found. The ContentType
                // check is performed since a request for a non-existent
                // image file might be redirected to a 404-page, which woulds
                // yield the StatusCode "OK", even though the image was not
                // found.
                if ((response.StatusCode == HttpStatusCode.OK ||
                    response.StatusCode == HttpStatusCode.Moved ||
                    response.StatusCode == HttpStatusCode.Redirect) &&
                    response.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
                {

                    var pathToSave = String.Format("{0}{1}.jpg", imgDir, title);
                    var url = String.Format("{0}{1}.jpg", ImgPathURL, title);
                    // if the remote file was found, download oits
                    using (var inputStream = response.GetResponseStream())
                    using (Stream outputStream = File.OpenWrite(pathToSave))
                    {
                        var buffer = new byte[4096];
                        int bytesRead;
                        do
                        {
                            bytesRead = inputStream.Read(buffer, 0, buffer.Length);
                            outputStream.Write(buffer, 0, bytesRead);
                        } while (bytesRead != 0);
                    }
                    return url;
                }
                else
                    return uri;
            }
            catch (Exception)
            {
                return uri;
            }
        }

        // update a given recipe
        private void updateRecipe(RecipeObj recipe)
        {
            // go through the recipe metadata and create/update if necessary
            createRecipeRecord(recipe);

            // ingredients
            addIngredients(recipe);

            // methods
            addMethods(recipe);

            // cuisines
            addCuisines(recipe);

            // mealtypess
            addMealTypes(recipe);

            // facts
            addFacts(recipe);

            // divisions        
            addDivisions(recipe);
        }

        // work out if the Division already exists, if it does then use it for the mapping table, else create it
        private void addDivisions(RecipeObj recipe)
        {
            using (var context = new WoolworthsDBDataContext())
            {
                // clear the current mappings
                var maps = context.RecipeDivisionMaps.Where(r => r.RecipeID == recipe.RecipeID);
                context.RecipeDivisionMaps.DeleteAllOnSubmit(maps);
                context.SubmitChanges();

                if (recipe.ValidDivisions == null)
                    return;

                foreach (var division in recipe.ValidDivisions)
                {
                    // create or retrieve the RecipeCuisine.ID value
                    var master = context.RecipeDivisions.SingleOrDefault(c => c.Description == division.ValidDivision);
                    var recipeDivisionID = 0;
                    if (master == null)
                    {
                        var toCreate = new RecipeDivision()
                        {
                            Description = division.ValidDivision
                        };
                        context.RecipeDivisions.InsertOnSubmit(toCreate);
                        context.SubmitChanges();
                        recipeDivisionID = toCreate.ID;
                    }
                    else
                    {
                        recipeDivisionID = master.ID;
                    }

                    // add to the mapping table
                    var map = new RecipeDivisionMap()
                    {
                        RecipeDivisionID = recipeDivisionID,
                        RecipeID = recipe.RecipeID
                    };
                    context.RecipeDivisionMaps.InsertOnSubmit(map);
                    context.SubmitChanges();
                }
            }
        }

        // work out if the Fact already exists, if it does then use it for the mapping table, else create it
        private static void addFacts(RecipeObj recipe)
        {
            using (var context = new WoolworthsDBDataContext())
            {
                // clear the current mappings
                var maps = context.RecipeFactMaps.Where(r => r.RecipeID == recipe.RecipeID);
                context.RecipeFactMaps.DeleteAllOnSubmit(maps);
                context.SubmitChanges();

                if (recipe.Facts == null)
                    return;

                foreach (var factType in recipe.Facts)
                {
                    // create or retrieve the RecipeCuisine.ID value
                    var master = context.RecipeFacts.SingleOrDefault(c => c.Description == factType);
                    var recipeFactID = 0;
                    if (master == null)
                    {
                        var toCreate = new RecipeFact()
                        {
                            Description = factType
                        };
                        context.RecipeFacts.InsertOnSubmit(toCreate);
                        context.SubmitChanges();
                        recipeFactID = toCreate.ID;
                    }
                    else
                    {
                        recipeFactID = master.ID;
                    }

                    // add to the mapping table
                    var map = new RecipeFactMap()
                    {
                        RecipeFactID = recipeFactID,
                        RecipeID = recipe.RecipeID
                    };
                    context.RecipeFactMaps.InsertOnSubmit(map);
                    context.SubmitChanges();
                }
            }
        }

        // work out if the RecipeMealType already exists, if it does then use it for the mapping table, else create it
        private static void addMealTypes(RecipeObj recipe)
        {
            using (var context = new WoolworthsDBDataContext())
            {
                // clear the current mappings
                var maps = context.RecipeMealTypeMaps.Where(r => r.RecipeID == recipe.RecipeID);
                context.RecipeMealTypeMaps.DeleteAllOnSubmit(maps);
                context.SubmitChanges();

                if (recipe.MealTypes == null)
                    return;

                foreach (var mealType in recipe.MealTypes)
                {
                    // create or retrieve the RecipeCuisine.ID value
                    var master = context.RecipeMealTypes.SingleOrDefault(c => c.Description == mealType.MealType);
                    var recipeMealTypeID = 0;
                    if (master == null)
                    {
                        var toCreate = new RecipeMealType()
                        {
                            Description = mealType.MealType
                        };
                        context.RecipeMealTypes.InsertOnSubmit(toCreate);
                        context.SubmitChanges();
                        recipeMealTypeID = toCreate.ID;
                    }
                    else
                    {
                        recipeMealTypeID = master.ID;
                    }

                    // add to the mapping table
                    var map = new RecipeMealTypeMap()
                    {
                        RecipeMealTypeID = recipeMealTypeID,
                        RecipeID = recipe.RecipeID
                    };
                    context.RecipeMealTypeMaps.InsertOnSubmit(map);
                    context.SubmitChanges();
                }
            }
        }

        // work out if the cuisine already exists, if it does then use it for the mapping table, else create it
        private static void addCuisines(RecipeObj recipe)
        {
            using (var context = new WoolworthsDBDataContext())
            {
                // clear the current mappings
                var maps = context.RecipeCuisineMaps.Where(r => r.RecipeID == recipe.RecipeID);
                context.RecipeCuisineMaps.DeleteAllOnSubmit(maps);
                context.SubmitChanges();

                if (recipe.Cuisines == null)
                    return;

                foreach (var cuisine in recipe.Cuisines)
                {
                    // create or retrieve the RecipeCuisine.ID value
                    var master = context.RecipeCuisines.SingleOrDefault(c => c.Description == cuisine.Cuisine);
                    var recipeCuisineID = 0;
                    if (master == null)
                    {
                        var toCreate = new RecipeCuisine()
                        {
                            Description = cuisine.Cuisine
                        };
                        context.RecipeCuisines.InsertOnSubmit(toCreate);
                        context.SubmitChanges();
                        recipeCuisineID = toCreate.ID;
                    }
                    else
                    {
                        recipeCuisineID = master.ID;
                    }

                    // add to the mapping table
                    var map = new RecipeCuisineMap()
                    {
                        RecipeCuisineID = recipeCuisineID,
                        RecipeID = recipe.RecipeID
                    };
                    context.RecipeCuisineMaps.InsertOnSubmit(map);
                    context.SubmitChanges();
                }
            }
        }

        // clear the current list of methods for a given recipe and add the ones based on the file to ingest
        private static void addMethods(RecipeObj recipe)
        {
            // clear the current list
            using (var context = new WoolworthsDBDataContext())
            {
                var methodToDel = context.RecipeMethods.Where(r => r.RecipeID == recipe.RecipeID);
                context.RecipeMethods.DeleteAllOnSubmit(methodToDel);
                context.SubmitChanges();
            }

            if (recipe.Methods == null)
                return;

            using (var context = new WoolworthsDBDataContext())
            {
                foreach (var method in recipe.Methods.Select(obj => new RecipeMethod()
                {
                    Description = obj.Step,
                    Number = obj.StepID,
                    RecipeID = recipe.RecipeID
                }))
                {
                    context.RecipeMethods.InsertOnSubmit(method);
                }
                context.SubmitChanges();
            }
        }

        // clear the current list of ingredients for a given recipe and add the ones based on the file to ingest
        private static void addIngredients(RecipeObj recipe)
        {
            // clear the current list
            using (var context = new WoolworthsDBDataContext())
            {
                var ingredToDel = context.RecipeIngredients.Where(r => r.RecipeID == recipe.RecipeID);
                context.RecipeIngredients.DeleteAllOnSubmit(ingredToDel);
                context.SubmitChanges();
            }

            if (recipe.Ingredients == null)
                return;

            using (var context = new WoolworthsDBDataContext())
            {
                foreach (var ingred in recipe.Ingredients.Select(obj => new RecipeIngredient()
                {
                    Description = obj.Ingredient,
                    Number = obj.IngredientID,
                    ProductArticle = obj.ProductID,
                    RecipeID = recipe.RecipeID
                }))
                {
                    context.RecipeIngredients.InsertOnSubmit(ingred);
                }
                context.SubmitChanges();
            }
        }

        // delete a given recipe by id
        private static void deleteRecipe(int id)
        {
            using (var context = new WoolworthsDBDataContext())
            {
                context.p_DeleteRecipe(id);
            }
        }

        private static void updateRecipeAsFeatured(FeaturedRecipeObj featuredRecipe)
        {
            using (var context = new WoolworthsDBDataContext())
            {
                var recipe = context.Recipes.SingleOrDefault(r => r.ID == featuredRecipe.RecipeID);
                if (recipe != null)
                {
                    recipe.FeaturedStartDate = DateTime.Parse(featuredRecipe.STARTDATE);
                    context.SubmitChanges();
                }
            }
        }

        protected override bool ExecuteDataProcessingProc()
        {
            return true;
        }

        protected override void ExecuteResetProc()
        {
            return;
        }

        protected override string DataProcessProcName
        {
            get { return string.Empty; }
        }

        protected override string ResetProcName
        {
            get { return string.Empty; }
        }

    }
}
