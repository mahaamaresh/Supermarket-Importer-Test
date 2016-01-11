USE [Woolworths]
GO

/****** Object:  Table [dbo].[Staging_NutritionalInfo]    Script Date: 02/13/2013 11:20:38 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[Staging_NutritionalInfo](
	[Action] [varchar](1) NOT NULL,
	[ProductArticle] [varchar](18) NOT NULL,
	[MandatoryWarning] [varchar](100) NULL,
	[StorageRequirements] [varchar](100) NULL,
	[NutritionalClaim] [varchar](100) NULL,
	[CountryOfOrigin] [varchar](40) NULL,
	[ServesPerPack] [varchar](7) NULL,
	[ServingSize] [varchar](7) NULL,
	[ServingSizeUnit] [varchar](3) NULL,
	[NutrientValue01] [varchar](20) NOT NULL,
	[ServingQuantity01] [varchar](7) NOT NULL,
	[ServingUnit01] [varchar](3) NULL,
	[ServingQuantity100G01] [varchar](7) NOT NULL,
	[ServingUnit100G01] [varchar](3) NULL,
	[NutrientValue02] [varchar](20) NOT NULL,
	[ServingQuantity02] [varchar](7) NOT NULL,
	[ServingUnit02] [varchar](3) NULL,
	[ServingQuantity100G02] [varchar](7) NOT NULL,
	[ServingUnit100G02] [varchar](3) NULL,
	[NutrientValue03] [varchar](20) NOT NULL,
	[ServingQuantity03] [varchar](7) NOT NULL,
	[ServingUnit03] [varchar](3) NULL,
	[ServingQuantity100G03] [varchar](7) NOT NULL,
	[ServingUnit100G03] [varchar](3) NULL,
	[NutrientValue04] [varchar](20) NOT NULL,
	[ServingQuantity04] [varchar](7) NOT NULL,
	[ServingUnit04] [varchar](3) NULL,
	[ServingQuantity100G04] [varchar](7) NOT NULL,
	[ServingUnit100G04] [varchar](3) NULL,
	[NutrientValue05] [varchar](20) NOT NULL,
	[ServingQuantity05] [varchar](7) NOT NULL,
	[ServingUnit05] [varchar](3) NULL,
	[ServingQuantity100G05] [varchar](7) NOT NULL,
	[ServingUnit100G05] [varchar](3) NULL,
	[NutrientValue06] [varchar](20) NOT NULL,
	[ServingQuantity06] [varchar](7) NOT NULL,
	[ServingUnit06] [varchar](3) NULL,
	[ServingQuantity100G06] [varchar](7) NOT NULL,
	[ServingUnit100G06] [varchar](3) NULL,
	[NutrientValue07] [varchar](20) NOT NULL,
	[ServingQuantity07] [varchar](7) NOT NULL,
	[ServingUnit07] [varchar](3) NULL,
	[ServingQuantity100G07] [varchar](7) NOT NULL,
	[ServingUnit100G07] [varchar](3) NULL,
	[NutrientValue08] [varchar](20) NOT NULL,
	[ServingQuantity08] [varchar](7) NOT NULL,
	[ServingUnit08] [varchar](3) NULL,
	[ServingQuantity100G08] [varchar](7) NOT NULL,
	[ServingUnit100G08] [varchar](3) NULL,
	[NutrientValue09] [varchar](20) NOT NULL,
	[ServingQuantity09] [varchar](7) NOT NULL,
	[ServingUnit09] [varchar](3) NULL,
	[ServingQuantity100G09] [varchar](7) NOT NULL,
	[ServingUnit100G09] [varchar](3) NULL,
	[NutrientValue10] [varchar](20) NOT NULL,
	[ServingQuantity10] [varchar](7) NOT NULL,
	[ServingUnit10] [varchar](3) NULL,
	[ServingQuantity100G10] [varchar](7) NOT NULL,
	[ServingUnit100G10] [varchar](3) NULL,
	[NutrientValue11] [varchar](20) NOT NULL,
	[ServingQuantity11] [varchar](7) NOT NULL,
	[ServingUnit11] [varchar](3) NULL,
	[ServingQuantity100G11] [varchar](7) NOT NULL,
	[ServingUnit100G11] [varchar](3) NULL,
	[NutrientValue12] [varchar](20) NOT NULL,
	[ServingQuantity12] [varchar](7) NOT NULL,
	[ServingUnit12] [varchar](3) NULL,
	[ServingQuantity100G12] [varchar](7) NOT NULL,
	[ServingUnit100G12] [varchar](3) NULL,
	[NutrientValue13] [varchar](20) NOT NULL,
	[ServingQuantity13] [varchar](7) NOT NULL,
	[ServingUnit13] [varchar](3) NULL,
	[ServingQuantity100G13] [varchar](7) NOT NULL,
	[ServingUnit100G13] [varchar](3) NULL,
	[NutrientValue14] [varchar](20) NOT NULL,
	[ServingQuantity14] [varchar](7) NOT NULL,
	[ServingUnit14] [varchar](3) NULL,
	[ServingQuantity100G14] [varchar](7) NOT NULL,
	[ServingUnit100G14] [varchar](3) NULL,
	[IngredientLine01] [varchar](50) NULL,
	[IngredientLine02] [varchar](50) NULL,
	[IngredientLine03] [varchar](50) NULL,
	[IngredientLine04] [varchar](50) NULL,
	[IngredientLine05] [varchar](50) NULL,
	[IngredientLine06] [varchar](50) NULL,
	[IngredientLine07] [varchar](50) NULL,
	[IngredientLine08] [varchar](50) NULL,
	[IngredientLine09] [varchar](50) NULL,
	[IngredientLine10] [varchar](50) NULL,
	[IngredientLine11] [varchar](50) NULL,
	[IngredientLine12] [varchar](50) NULL,
	[IngredientLine13] [varchar](50) NULL,
	[IngredientLine14] [varchar](50) NULL,
	[IngredientLine15] [varchar](50) NULL,
	[IngredientLine16] [varchar](50) NULL,
	[IngredientLine17] [varchar](50) NULL,
	[IngredientLine18] [varchar](50) NULL
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO



USE [Woolworths]
GO

/****** Object:  Table [dbo].[NutritionalInfo]    Script Date: 02/13/2013 11:19:26 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[NutritionalInfo](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ProductArticle] [varchar](18) NOT NULL,
	[MandatoryWarning] [varchar](100) NULL,
	[StorageRequirements] [varchar](100) NULL,
	[NutritionalClaim] [varchar](100) NULL,
	[CountryOfOrigin] [varchar](40) NULL,
	[ServesPerPack] [decimal](18, 0) NULL,
	[ServingSize] [decimal](18, 0) NULL,
	[ServingSizeUnit] [varchar](3) NULL,
	[Nutrients] [varchar](max) NULL,
	[Ingredients] [varchar](max) NULL,
	[DateCreated] [datetime] NOT NULL,
	[DateUpdated] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = ON, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[NutritionalInfo]  WITH NOCHECK ADD FOREIGN KEY([ProductArticle])
REFERENCES [dbo].[Product] ([Article])
GO


