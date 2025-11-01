USE [ZoryaLIS]
GO
/****** Object:  Table [dbo].[AccuHealthParamMappings]    Script Date: 01-11-2025 10:17:19 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AccuHealthParamMappings](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[HIS_PARAMCODE] [nvarchar](max) NULL,
	[HIS_PARAMNAME] [nvarchar](max) NULL,
	[LIS_PARAMCODE] [nvarchar](max) NULL,
	[SPECIMEN] [nvarchar](max) NULL,
	[UNIT] [nvarchar](max) NULL,
	[EquipmentId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.AccuHealthParamMappings] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AccuHealthTestOrders]    Script Date: 01-11-2025 10:17:19 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AccuHealthTestOrders](
	[ROW_ID] [uniqueidentifier] NOT NULL,
	[isSynced] [bit] NOT NULL,
	[branch_ID] [uniqueidentifier] NOT NULL,
	[IPOPFLAG] [nvarchar](max) NULL,
	[PINNO] [nvarchar](max) NULL,
	[REF_VISITNO] [nvarchar](max) NULL,
	[ADMISSIONNO] [nvarchar](max) NULL,
	[REQDATETIME] [datetime] NULL,
	[TESTPROF_CODE] [nvarchar](max) NULL,
	[PROCESSED] [nvarchar](max) NULL,
	[PATFNAME] [nvarchar](max) NULL,
	[PATMNAME] [nvarchar](max) NULL,
	[PATLNAME] [nvarchar](max) NULL,
	[PAT_DOB] [nvarchar](max) NULL,
	[GENDER] [nvarchar](max) NULL,
	[PATAGE] [nvarchar](max) NULL,
	[AGEUNIT] [nvarchar](max) NULL,
	[DOC_NAME] [nvarchar](max) NULL,
	[PATIENTTYPECLASS] [nvarchar](max) NULL,
	[SEQNO] [nvarchar](max) NULL,
	[ADDDATE] [nvarchar](max) NULL,
	[ADDTIME] [nvarchar](max) NULL,
	[TITLE] [nvarchar](max) NULL,
	[LABNO] [nvarchar](max) NULL,
	[DATESTAMP] [datetime] NULL,
	[PARAMCODE] [nvarchar](max) NULL,
	[PARAMNAME] [nvarchar](max) NULL,
	[MRESULT] [nvarchar](max) NULL,
	[BC_PRINTED] [nvarchar](max) NULL,
	[Value] [nvarchar](max) NULL,
	[Status] [int] NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.AccuHealthTestOrders] PRIMARY KEY CLUSTERED 
(
	[ROW_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AccuHealthTestValues]    Script Date: 01-11-2025 10:17:19 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AccuHealthTestValues](
	[ROW_ID] [uniqueidentifier] NOT NULL,
	[isSynced] [bit] NOT NULL,
	[SRNO] [nvarchar](max) NULL,
	[SDATE] [datetime] NULL,
	[SAMPLEID] [nvarchar](max) NULL,
	[TESTID] [nvarchar](max) NULL,
	[MACHINEID] [nvarchar](max) NULL,
	[SUFFIX] [nvarchar](max) NULL,
	[TRANSFERFLAG] [nvarchar](max) NULL,
	[TMPVALUE] [nvarchar](max) NULL,
	[DESCRIPTION] [nvarchar](max) NULL,
	[RUNDATE] [datetime] NOT NULL,
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
 CONSTRAINT [PK_dbo.AccuHealthTestValues] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetRoles]    Script Date: 01-11-2025 10:17:19 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoles](
	[Id] [nvarchar](128) NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
 CONSTRAINT [PK_dbo.AspNetRoles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserClaims]    Script Date: 01-11-2025 10:17:19 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](128) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.AspNetUserClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserLogins]    Script Date: 01-11-2025 10:17:19 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserLogins](
	[LoginProvider] [nvarchar](128) NOT NULL,
	[ProviderKey] [nvarchar](128) NOT NULL,
	[UserId] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_dbo.AspNetUserLogins] PRIMARY KEY CLUSTERED 
(
	[LoginProvider] ASC,
	[ProviderKey] ASC,
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserRoles]    Script Date: 01-11-2025 10:17:19 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserRoles](
	[UserId] [nvarchar](128) NOT NULL,
	[RoleId] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_dbo.AspNetUserRoles] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUsers]    Script Date: 01-11-2025 10:17:19 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUsers](
	[Id] [nvarchar](128) NOT NULL,
	[FirstName] [nvarchar](max) NULL,
	[LastName] [nvarchar](max) NULL,
	[AlternativeEmail] [nvarchar](256) NULL,
	[DOB] [datetime] NULL,
	[Country] [nvarchar](256) NULL,
	[State] [nvarchar](256) NULL,
	[Address] [nvarchar](256) NULL,
	[Zip] [nvarchar](10) NULL,
	[AreaOfInterest] [nvarchar](256) NULL,
	[Qualification] [nvarchar](256) NULL,
	[IsBlocked] [bit] NOT NULL,
	[Email] [nvarchar](256) NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[PasswordHash] [nvarchar](max) NULL,
	[SecurityStamp] [nvarchar](max) NULL,
	[PhoneNumber] [nvarchar](max) NULL,
	[PhoneNumberConfirmed] [bit] NOT NULL,
	[TwoFactorEnabled] [bit] NOT NULL,
	[LockoutEndDateUtc] [datetime] NULL,
	[LockoutEnabled] [bit] NOT NULL,
	[AccessFailedCount] [int] NOT NULL,
	[UserName] [nvarchar](256) NOT NULL,
 CONSTRAINT [PK_dbo.AspNetUsers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ClientApplication]    Script Date: 01-11-2025 10:17:19 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ClientApplication](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
	[Description] [nvarchar](1024) NOT NULL,
	[AccessKey] [nvarchar](50) NOT NULL,
	[ActivityDate] [datetime2](7) NULL,
	[ActivityMember] [nvarchar](256) NULL,
	[RefreshTokenLifeTime] [int] NOT NULL,
	[AllowedOrigin] [nvarchar](100) NULL,
	[ModulesAPI] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.ClientApplication] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EquipmentHeartBeat]    Script Date: 01-11-2025 10:17:19 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EquipmentHeartBeat](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AccessKey] [nvarchar](50) NOT NULL,
	[IsAlive] [bit] NOT NULL,
	[CreatedBy] [nvarchar](max) NULL,
	[CreatedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.EquipmentHeartBeat] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EquipmentMaster]    Script Date: 01-11-2025 10:17:19 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EquipmentMaster](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NULL,
	[Model] [nvarchar](max) NULL,
	[AccessKey] [nvarchar](50) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedBy] [nvarchar](max) NULL,
	[CreatedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.EquipmentMaster] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LisTestValues]    Script Date: 01-11-2025 10:17:19 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LisTestValues](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[REF_VISITNO] [nvarchar](max) NULL,
	[PARAMCODE] [nvarchar](max) NULL,
	[Value] [nvarchar](max) NULL,
	[CreatedAt] [datetime] NOT NULL,
	[Equipment] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.LisTestValues] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MigrationHistory]    Script Date: 01-11-2025 10:17:19 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MigrationHistory](
	[Migration_PK] [nvarchar](150) NOT NULL,
	[ContextKey] [nvarchar](300) NOT NULL,
	[Model] [varbinary](max) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK_dbo.MigrationHistory] PRIMARY KEY CLUSTERED 
(
	[Migration_PK] ASC,
	[ContextKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RefreshTokens]    Script Date: 01-11-2025 10:17:19 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RefreshTokens](
	[Id] [nvarchar](128) NOT NULL,
	[Subject] [nvarchar](50) NOT NULL,
	[ClientId] [nvarchar](50) NOT NULL,
	[IssuedUtc] [datetime] NOT NULL,
	[ExpiresUtc] [datetime] NOT NULL,
	[ProtectedTicket] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_dbo.RefreshTokens] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RoleModuleMappings]    Script Date: 01-11-2025 10:17:19 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RoleModuleMappings](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CanAdd] [bit] NOT NULL,
	[CanEdit] [bit] NOT NULL,
	[CanAuthorize] [bit] NOT NULL,
	[CanDelete] [bit] NOT NULL,
	[CanView] [bit] NOT NULL,
	[CanReject] [bit] NOT NULL,
	[ModuleId] [bigint] NULL,
	[RoleId] [nvarchar](128) NOT NULL,
	[ApplicationId] [int] NULL,
 CONSTRAINT [PK_dbo.RoleModuleMappings] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserApplicationMappings]    Script Date: 01-11-2025 10:17:19 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserApplicationMappings](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ClientApplicationId] [int] NULL,
	[UserId] [nvarchar](128) NULL,
 CONSTRAINT [PK_dbo.UserApplicationMappings] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserModules]    Script Date: 01-11-2025 10:17:19 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserModules](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](128) NOT NULL,
	[Url] [nvarchar](128) NOT NULL,
	[Order] [int] NOT NULL,
	[ApplicationId] [int] NOT NULL,
	[IsSyatem] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.UserModules] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[AccuHealthTestOrders] ADD  DEFAULT ((0)) FOR [Status]
GO
ALTER TABLE [dbo].[AccuHealthTestOrders] ADD  DEFAULT ('1900-01-01T00:00:00.000') FOR [CreatedAt]
GO
ALTER TABLE [dbo].[AccuHealthParamMappings]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AccuHealthParamMappings_dbo.EquipmentMaster_EquipmentId] FOREIGN KEY([EquipmentId])
REFERENCES [dbo].[EquipmentMaster] ([Id])
GO
ALTER TABLE [dbo].[AccuHealthParamMappings] CHECK CONSTRAINT [FK_dbo.AccuHealthParamMappings_dbo.EquipmentMaster_EquipmentId]
GO
ALTER TABLE [dbo].[AspNetUserClaims]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserClaims] CHECK CONSTRAINT [FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserLogins]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserLogins] CHECK CONSTRAINT [FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[RoleModuleMappings]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RoleModuleMappings_dbo.AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[RoleModuleMappings] CHECK CONSTRAINT [FK_dbo.RoleModuleMappings_dbo.AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[RoleModuleMappings]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RoleModuleMappings_dbo.ClientApplication_ApplicationId] FOREIGN KEY([ApplicationId])
REFERENCES [dbo].[ClientApplication] ([Id])
GO
ALTER TABLE [dbo].[RoleModuleMappings] CHECK CONSTRAINT [FK_dbo.RoleModuleMappings_dbo.ClientApplication_ApplicationId]
GO
ALTER TABLE [dbo].[RoleModuleMappings]  WITH CHECK ADD  CONSTRAINT [FK_dbo.RoleModuleMappings_dbo.UserModules_ModuleId] FOREIGN KEY([ModuleId])
REFERENCES [dbo].[UserModules] ([Id])
GO
ALTER TABLE [dbo].[RoleModuleMappings] CHECK CONSTRAINT [FK_dbo.RoleModuleMappings_dbo.UserModules_ModuleId]
GO
ALTER TABLE [dbo].[UserApplicationMappings]  WITH CHECK ADD  CONSTRAINT [FK_dbo.UserApplicationMappings_dbo.AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
GO
ALTER TABLE [dbo].[UserApplicationMappings] CHECK CONSTRAINT [FK_dbo.UserApplicationMappings_dbo.AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[UserApplicationMappings]  WITH CHECK ADD  CONSTRAINT [FK_dbo.UserApplicationMappings_dbo.ClientApplication_ClientApplicationId] FOREIGN KEY([ClientApplicationId])
REFERENCES [dbo].[ClientApplication] ([Id])
GO
ALTER TABLE [dbo].[UserApplicationMappings] CHECK CONSTRAINT [FK_dbo.UserApplicationMappings_dbo.ClientApplication_ClientApplicationId]
GO
ALTER TABLE [dbo].[UserModules]  WITH CHECK ADD  CONSTRAINT [FK_dbo.UserModules_dbo.ClientApplication_ApplicationId] FOREIGN KEY([ApplicationId])
REFERENCES [dbo].[ClientApplication] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[UserModules] CHECK CONSTRAINT [FK_dbo.UserModules_dbo.ClientApplication_ApplicationId]
GO
