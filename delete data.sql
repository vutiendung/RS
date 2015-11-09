use BigData
delete top (2400) from [dbo].[TOULEMBAL$Item]
delete top (4500) from [dbo].[TOULEMBAL$Customer]
--delete transactions not exist item and customer
use BigData
go
declare del_item cursor
	for select top 6500 [No_],[Sell-to Customer No_] from [dbo].[TOULEMBAL$Order Ledger Entries]
declare @no varchar(20)
declare @sell varchar(20)
open del_item
fetch NEXT from del_item into @no,@sell
begin
	if(@no not in (select No_ from [dbo].[TOULEMBAL$Item]))
		delete from [dbo].[TOULEMBAL$Order Ledger Entries] where [No_] = @no
	if(@sell not in (select No_ from [dbo].[TOULEMBAL$Customer]))
		delete from [dbo].[TOULEMBAL$Order Ledger Entries] where [Sell-to Customer No_] = @sell
		
	fetch NEXT from del_item into @no,@sell
end
close del_item
DEALLOCATE del_item