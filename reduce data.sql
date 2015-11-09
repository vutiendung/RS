--reduce data
---Customer----------------------------------------------------
use SOURCE
go
declare custom CURSOR
	for SELECT 
				[No_], 
				[Name],
				[Customer Segment],
				[Customer Category],
				[Creation Date], 
				[CA KOMPASS]
			From
				[dbo].[TOULEMBAL$Customer]

open custom
use BigData
go
declare @No[varchar](20)
declare @Name[varchar](50)
declare @Customer_Category [int]
declare @Customer_Segment [varchar](50)
declare @Creation_Date [datetime]
declare @CA_KOMPASS [decimal](38, 20)
FETCH NEXT FROM custom 
INTO
	@No,
	@Name,
	@Customer_Segment,
	@Customer_Category,
	@Creation_Date,
	@CA_KOMPASS
WHILE @@FETCH_STATUS = 0
begin
	insert into [BigData].[dbo].[TOULEMBAL$Customer]
	values(
			@No,
			@Name,
			@Customer_Category,
			@Customer_Segment,
			@Creation_Date,
			@CA_KOMPASS)
	FETCH NEXT FROM custom
	INTO
		@No,
		@Name,
		@Customer_Segment,
		@Customer_Category,
		@Creation_Date,
		@CA_KOMPASS
end
close custom
DEALLOCATE custom
---Item----------------------------------------------------
use SOURCE
go
declare item CURSOR
	for SELECT 
			[No_], 
			[Description], 
			[Code Métaproduit photo],
			[Code Famille article photo], 
			[Code Univers photo], 
			[Creation Date]
		From
			[dbo].[TOULEMBAL$Item]

open item
use BigData
go 
declare @No_ [varchar](20)
declare @Description [varchar](30)
declare @Code_Métaproduit_photo [varchar](50)
declare @Code_Famille_article_photo [varchar](50)
declare @Code_Univers_photo [varchar](50)
declare @Creation_Date [datetime]

FETCH NEXT FROM item 
INTO
	@No_,
	@Description,
	@Code_Métaproduit_photo,
	@Code_Famille_article_photo,
	@Code_Univers_photo,
	@Creation_Date
WHILE @@FETCH_STATUS = 0
begin
	insert into [BigData].[dbo].[TOULEMBAL$Item]
	values(
			@No_,
			@Description,
			@Code_Métaproduit_photo,
			@Code_Famille_article_photo,
			@Code_Univers_photo,
			@Creation_Date
			)
	FETCH NEXT FROM item
	INTO
		@No_,
		@Description,
		@Code_Métaproduit_photo,
		@Code_Famille_article_photo,
		@Code_Univers_photo,
		@Creation_Date
end
close item
DEALLOCATE item

--Transactions-----------------------------------------
use SOURCE
go
declare transactions CURSOR
	for SELECT
			[Sell-to Customer No_], 
			[No_],
			[Document Date] [Date], 
			[Quantity Shipped]
		FROM
			[TOULEMBAL$Order Ledger Entries]


open transactions
use BigData
go 
declare @Sell_to_Customer_No_ [varchar](20)
declare @No_ varchar(20)
declare @Document_Date [datetime]
declare @Quantity_Shipped [decimal](38, 20)

FETCH NEXT FROM transactions 
INTO
	@Sell_to_Customer_No_,
	@No_,
	@Document_Date,
	@Quantity_Shipped
WHILE @@FETCH_STATUS = 0
begin
	insert into [BigData].[dbo].[TOULEMBAL$Order Ledger Entries]
	values(
			@Sell_to_Customer_No_,
			@No_,
			@Document_Date,
			@Quantity_Shipped
			)
	FETCH NEXT FROM transactions
	INTO
		@Sell_to_Customer_No_,
		@No_,
		@Document_Date,
		@Quantity_Shipped
end
close transactions
DEALLOCATE transactions
--Price-----------------------------
use SOURCE
go
declare Price CURSOR
	for SELECT 
			[Item No_],
			[Minimum Quantity],
			[Unit Price],
			[Ending Date]
		FROM
 			[TOULEMBAL$Sales Price]

open Price
use BigData
go 

declare @Item_No_ [varchar](20)
declare @Minimum_Quantity [decimal](38, 20)
declare @Unit_Price [decimal](38, 20)
declare @Ending_Date [datetime]

FETCH NEXT FROM Price 
INTO
	@Item_No_,
	@Minimum_Quantity,
	@Unit_Price,
	@Ending_Date
WHILE @@FETCH_STATUS = 0
begin
	insert into [BigData].[dbo].[TOULEMBAL$Sales Price]
	values(
			@Item_No_,
			@Minimum_Quantity,
			@Unit_Price,
			@Ending_Date
			)
	FETCH NEXT FROM Price
	INTO
		@Item_No_,
		@Minimum_Quantity,
		@Unit_Price,
		@Ending_Date
end
close Price
DEALLOCATE Price