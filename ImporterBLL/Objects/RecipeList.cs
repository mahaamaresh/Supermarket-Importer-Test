using System;

namespace ImporterBLL.Objects
{
    [Serializable]
    public class RecipeList
    {
        public RecipeObj[] recipes { get; set; }
    }

    [Serializable]
    public class FeaturedRecipeList
    {
        public FeaturedRecipeObj[] FeaturedRecipe { get; set; }
    }

    [Serializable]
    public class IngredientObj
    {
        public int IngredientID { get; set; }
        public string Ingredient { get; set; }
        public string ProductID { get; set; }
    }
    [Serializable]
    public class Method
    {
        public int StepID { get; set; }
        public string Step { get; set; }
    }
    [Serializable]
    public class CuisineObj
    {
        public string Cuisine { get; set; }
    }
    [Serializable]
    public class MealTypeObj
    {
        public string MealType { get; set; }
    }
    [Serializable]
    public class ValidDivisionObj
    {
        public string ValidDivision { get; set; }
    }
    [Serializable]
    public class RecipeObj
    {
        public int RecipeID { get; set; }
        public string Recipe { get; set; }
        public string Title { get; set; }
        public string Action { get; set; }
        public string ImageURL { get; set; }
        public string Description { get; set; }
        public string PreparationTime { get; set; }
        public string CookingTime { get; set; }
        public string TotalTime { get; set; }
        public string Serves { get; set; }
        public string Tip { get; set; }

        public IngredientObj[] Ingredients { get; set; }
        public Method[] Methods { get; set; }
        public CuisineObj[] Cuisines { get; set; }
        public MealTypeObj[] MealTypes { get; set; }
        public String[] Facts { get; set; }
        /* not currently used */
        public ValidDivisionObj[] ValidDivisions { get; set; }
    }

    [Serializable]
    public class FeaturedRecipeObj
    {
        public string Division { get; set; }
        public int RecipeID { get; set; }
        public string Title { get; set; }
        public string STARTDATE { get; set; }

    }
}
