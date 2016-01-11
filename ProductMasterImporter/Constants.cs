namespace WoolworthsBLL
{
    public class Constants
    {
        public const string EmptyOfferNo = "0000000";
        public const decimal GSTAustralia = 1.1m;
        public const decimal GSTNewZealand = 1.15m;

        public enum Services
        {
            ProductMaster=1,
            ProductWebCategory=2,
            ProductPriceRanging=3,
            ProductLocation=4,
            WeeklySpecials=5,
            MultibuyOffers=6,
            RelevantSpecials=7,
            TargetedOffers=8,
            Images=9,
            ProductLocationDaily = 10,
            HealthWellbeing = 11
        }

        public enum DeviceType
        {
            iOS,
            Android,
            Other
        }

        public enum FlatFileType
        {
            ProductMaster = 1,
            ProductWebCategory = 2,
            ProductPriceRanging = 3,
            ProductLocation = 4,
            WeeklySpecials = 5,
            MultibuyOffers = 6,
            RelevantSpecials = 7,
            TargetedOffers = 8,
            Images = 9,
            ProductLocationDaily = 10,
            HealthWellbeing = 11,
            Recipe = 12,
            BonusPoints = 13
        }

        public enum ProcessState
        {
            Error = -1,
            Unprocessed = 0,
            Processed = 1
        }

        public enum SMSType
        {
            FileNotArrived,
            Error
        }

        public enum ResponseStatus
        {
            VersionNotSupported = -3,
            CustomerCreationDisabled = -2,
            Error = -1,
            Success = 0,
            CategoryNotFound = 1,
            ProductNotFound = 2,
            OffersNotFound = 3,
            SpecialsNotFound = 4,
            LocationNoResults = 5,
            SettingNotFound = 6,
            HealthWellbeingTagNotFound = 7,
            ShoppingListNotFound = 8,
            UnableToCreateShoppingList = 9,
            RecipeNotFound = 10,
            NoFeaturedRecipe = 11,
            DivisionNotFound = 12,
            ProductIsStaffDiscountCard = 13,
            ProductIsERDCard = 14,
            IsShelfTalkerLabel = 15
        }

        public enum WaitReason
        {
            None = 0,
            FileDoesNotExist = 1
        }

        public enum ProcessOutcome
        {
            Success = 0,
            ExpectedFileNotFoundError = 1,
            TimeOfDayExceededError = 2,
            BulkInsertError = 3,
            UnzipError = 4,
            ValidatePropertyError = 5,
            UnhandledExceptionError = 6,
            HandledSQLExceptionError = 7,
            EndOfDayHasPassedError = 8,
            ArchiveError = 9,
            ForcedStopped = 10,
            AdhocSuccess = 11
        }

        public enum StoreSearchBy
        {
            Postcode = 0,
            Suburb,
            GeoLocation
        }

        public enum ShoppingListType
        {
            Share = 0,
            Sync
        }
    }
}
