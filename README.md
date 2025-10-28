# zorya-lis
1. Change Connection string within 
	1.1 App.config in Dxi800
	1.2 App.config in DataModel

2. EF Code first Data migration
------------------------------------------------
#### Applied Once - Not required any more ---
Enable-Migrations -ProjectName Lis.Api -StartUpProjectName Lis.Api
Enable-Migrations -ProjectName LIS.DataAccess -StartUpProjectName Lis.Api
Enable-Migrations -ProjectName HIS.Api.Simujlator -StartUpProjectName HIS.Api.Simujlator

#### For Lis.Api
Add-Migration -ProjectName Lis.Api -StartUpProjectName Lis.Api
Update-Database -configuration Lis.Api.DataContextMigrations.Configuration -Verbose -ProjectName Lis.Api -StartUpProjectName Lis.Api


#### For LIS Data
Add-Migration -ProjectName LIS.DataAccess -StartUpProjectName Lis.Api
Update-Database -configuration LIS.DataAccess.Migrations.Configuration -Verbose -ProjectName LIS.DataAccess -StartUpProjectName Lis.Api

