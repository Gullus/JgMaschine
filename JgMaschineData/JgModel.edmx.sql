
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 04/04/2017 17:12:06
-- Generated from EDMX file: C:\Entwicklung\JgMaschine\JgMaschineData\JgModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [JgMaschine];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_tabMaschinetabAnmeldung]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tabAnmeldungMaschineSet] DROP CONSTRAINT [FK_tabMaschinetabAnmeldung];
GO
IF OBJECT_ID(N'[dbo].[FK_tabBedienertabAnmeldung]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tabAnmeldungMaschineSet] DROP CONSTRAINT [FK_tabBedienertabAnmeldung];
GO
IF OBJECT_ID(N'[dbo].[FK_tabBedienertabReparatur]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tabReparaturSet] DROP CONSTRAINT [FK_tabBedienertabReparatur];
GO
IF OBJECT_ID(N'[dbo].[FK_tabBedienertabReparatur1]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tabReparaturSet] DROP CONSTRAINT [FK_tabBedienertabReparatur1];
GO
IF OBJECT_ID(N'[dbo].[FK_tabMaschinetabReparatur]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tabReparaturSet] DROP CONSTRAINT [FK_tabMaschinetabReparatur];
GO
IF OBJECT_ID(N'[dbo].[FK_tabMaschinetabProtokoll]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tabProtokollSet] DROP CONSTRAINT [FK_tabMaschinetabProtokoll];
GO
IF OBJECT_ID(N'[dbo].[FK_tabStandorttabMaschine]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tabMaschineSet] DROP CONSTRAINT [FK_tabStandorttabMaschine];
GO
IF OBJECT_ID(N'[dbo].[FK_tabBedienertabArbeitszeit]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tabArbeitszeitSet] DROP CONSTRAINT [FK_tabBedienertabArbeitszeit];
GO
IF OBJECT_ID(N'[dbo].[FK_tabBauteiltabBediener_tabBauteil]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tabBauteiltabBediener] DROP CONSTRAINT [FK_tabBauteiltabBediener_tabBauteil];
GO
IF OBJECT_ID(N'[dbo].[FK_tabBauteiltabBediener_tabBediener]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tabBauteiltabBediener] DROP CONSTRAINT [FK_tabBauteiltabBediener_tabBediener];
GO
IF OBJECT_ID(N'[dbo].[FK_tabStandorttabArbeitszeit1]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tabArbeitszeitSet] DROP CONSTRAINT [FK_tabStandorttabArbeitszeit1];
GO
IF OBJECT_ID(N'[dbo].[FK_tabStandorttabBediener]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tabBedienerSet] DROP CONSTRAINT [FK_tabStandorttabBediener];
GO
IF OBJECT_ID(N'[dbo].[FK_tabBauteiltabMaschine]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tabMaschineSet] DROP CONSTRAINT [FK_tabBauteiltabMaschine];
GO
IF OBJECT_ID(N'[dbo].[FK_tabBauteiltabMaschine1]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tabBauteilSet] DROP CONSTRAINT [FK_tabBauteiltabMaschine1];
GO
IF OBJECT_ID(N'[dbo].[FK_tabReparaturtabAnmeldungReparatur]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tabAnmeldungReparaturSet] DROP CONSTRAINT [FK_tabReparaturtabAnmeldungReparatur];
GO
IF OBJECT_ID(N'[dbo].[FK_tabBedienertabAnmeldungReparatur]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tabAnmeldungReparaturSet] DROP CONSTRAINT [FK_tabBedienertabAnmeldungReparatur];
GO
IF OBJECT_ID(N'[dbo].[FK_tabReparaturtabMaschine]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tabMaschineSet] DROP CONSTRAINT [FK_tabReparaturtabMaschine];
GO
IF OBJECT_ID(N'[dbo].[FK_tabArbeitszeittabBediener]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tabBedienerSet] DROP CONSTRAINT [FK_tabArbeitszeittabBediener];
GO
IF OBJECT_ID(N'[dbo].[FK_tabMaschinetabAnmeldungMaschine]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tabAnmeldungMaschineSet] DROP CONSTRAINT [FK_tabMaschinetabAnmeldungMaschine];
GO
IF OBJECT_ID(N'[dbo].[FK_tabArbeitszeitAuswertungtabArbeitszeit]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tabArbeitszeitSet] DROP CONSTRAINT [FK_tabArbeitszeitAuswertungtabArbeitszeit];
GO
IF OBJECT_ID(N'[dbo].[FK_tabBedienertabArbeitszeitAuswertung]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tabArbeitszeitAuswertungSet] DROP CONSTRAINT [FK_tabBedienertabArbeitszeitAuswertung];
GO
IF OBJECT_ID(N'[dbo].[FK_tabArbeitszeitAuswertungtabArbeitszeitTag]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tabArbeitszeitTagSet] DROP CONSTRAINT [FK_tabArbeitszeitAuswertungtabArbeitszeitTag];
GO
IF OBJECT_ID(N'[dbo].[FK_tabStandorttabArbeitzzeitRunden]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tabArbeitszeitRundenSet] DROP CONSTRAINT [FK_tabStandorttabArbeitzzeitRunden];
GO
IF OBJECT_ID(N'[dbo].[FK_tabStandorttabArbeitszeitTerminal]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tabArbeitszeitTerminalSet] DROP CONSTRAINT [FK_tabStandorttabArbeitszeitTerminal];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[tabMaschineSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tabMaschineSet];
GO
IF OBJECT_ID(N'[dbo].[tabBedienerSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tabBedienerSet];
GO
IF OBJECT_ID(N'[dbo].[tabBauteilSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tabBauteilSet];
GO
IF OBJECT_ID(N'[dbo].[tabAnmeldungMaschineSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tabAnmeldungMaschineSet];
GO
IF OBJECT_ID(N'[dbo].[tabProtokollSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tabProtokollSet];
GO
IF OBJECT_ID(N'[dbo].[tabReparaturSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tabReparaturSet];
GO
IF OBJECT_ID(N'[dbo].[tabStandortSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tabStandortSet];
GO
IF OBJECT_ID(N'[dbo].[tabAuswertungSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tabAuswertungSet];
GO
IF OBJECT_ID(N'[dbo].[tabArbeitszeitSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tabArbeitszeitSet];
GO
IF OBJECT_ID(N'[dbo].[tabAnmeldungReparaturSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tabAnmeldungReparaturSet];
GO
IF OBJECT_ID(N'[dbo].[tabArbeitszeitTagSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tabArbeitszeitTagSet];
GO
IF OBJECT_ID(N'[dbo].[tabFeiertageSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tabFeiertageSet];
GO
IF OBJECT_ID(N'[dbo].[tabArbeitszeitAuswertungSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tabArbeitszeitAuswertungSet];
GO
IF OBJECT_ID(N'[dbo].[tabSollStundenSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tabSollStundenSet];
GO
IF OBJECT_ID(N'[dbo].[tabPausenzeitSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tabPausenzeitSet];
GO
IF OBJECT_ID(N'[dbo].[tabArbeitszeitRundenSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tabArbeitszeitRundenSet];
GO
IF OBJECT_ID(N'[dbo].[tabArbeitszeitTerminalSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tabArbeitszeitTerminalSet];
GO
IF OBJECT_ID(N'[dbo].[tabBauteiltabBediener]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tabBauteiltabBediener];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'tabMaschineSet'
CREATE TABLE [dbo].[tabMaschineSet] (
    [Id] uniqueidentifier  NOT NULL,
    [MaschinenName] nvarchar(120)  NOT NULL,
    [MaschinenArt] tinyint  NOT NULL,
    [IstStangenschneider] bit  NOT NULL,
    [MaschineAdresse] nvarchar(100)  NULL,
    [MaschinePortnummer] int  NULL,
    [ScannerNummer] nvarchar(20)  NULL,
    [ScannerMitDisplay] bit  NOT NULL,
    [ProdDatenabfrage] bit  NOT NULL,
    [Bemerkung] nvarchar(max)  NULL,
    [VorgabeProStunde] decimal(8,3)  NOT NULL,
    [Status] tinyint  NOT NULL,
    [DatenAbgleich_Datum] datetime  NOT NULL,
    [DatenAbgleich_Status] tinyint  NOT NULL,
    [DatenAbgleich_Bearbeiter] nvarchar(60)  NOT NULL,
    [DatenAbgleich_Geloescht] bit  NOT NULL,
    [fStandort] uniqueidentifier  NOT NULL,
    [fAktivBauteil] uniqueidentifier  NULL,
    [fAktivReparatur] uniqueidentifier  NULL
);
GO

-- Creating table 'tabBedienerSet'
CREATE TABLE [dbo].[tabBedienerSet] (
    [Id] uniqueidentifier  NOT NULL,
    [NachName] nvarchar(120)  NOT NULL,
    [VorName] nvarchar(120)  NOT NULL,
    [Bemerkung] nvarchar(max)  NULL,
    [MatchCode] nvarchar(120)  NULL,
    [Urlaubstage] tinyint  NOT NULL,
    [IdBuchhaltung] nvarchar(10)  NULL,
    [AuszahlungGehalt] tinyint  NOT NULL,
    [Status] tinyint  NOT NULL,
    [DatenAbgleich_Datum] datetime  NOT NULL,
    [DatenAbgleich_Status] tinyint  NOT NULL,
    [DatenAbgleich_Bearbeiter] nvarchar(60)  NOT NULL,
    [DatenAbgleich_Geloescht] bit  NOT NULL,
    [fAktivArbeitszeit] uniqueidentifier  NULL,
    [fStandort] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'tabBauteilSet'
CREATE TABLE [dbo].[tabBauteilSet] (
    [Id] uniqueidentifier  NOT NULL,
    [DatumStart] datetime  NOT NULL,
    [DatumEnde] datetime  NULL,
    [ScannZeit] datetime  NULL,
    [IstVorfertigung] bit  NOT NULL,
    [AnzahlBediener] tinyint  NOT NULL,
    [IstHandeingabe] bit  NOT NULL,
    [BvbsCode] nvarchar(max)  NULL,
    [IdStahlBauteil] int  NOT NULL,
    [BtGewicht] decimal(18,0)  NOT NULL,
    [DatenAbgleich_Datum] datetime  NOT NULL,
    [DatenAbgleich_Status] tinyint  NOT NULL,
    [DatenAbgleich_Bearbeiter] nvarchar(60)  NOT NULL,
    [DatenAbgleich_Geloescht] bit  NOT NULL,
    [fMaschine] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'tabAnmeldungMaschineSet'
CREATE TABLE [dbo].[tabAnmeldungMaschineSet] (
    [Id] uniqueidentifier  NOT NULL,
    [Anmeldung] datetime  NOT NULL,
    [Abmeldung] datetime  NULL,
    [ManuelleAnmeldung] bit  NOT NULL,
    [ManuelleAbmeldung] bit  NOT NULL,
    [DatenAbgleich_Datum] datetime  NOT NULL,
    [DatenAbgleich_Status] tinyint  NOT NULL,
    [DatenAbgleich_Bearbeiter] nvarchar(60)  NOT NULL,
    [DatenAbgleich_Geloescht] bit  NOT NULL,
    [fBediener] uniqueidentifier  NOT NULL,
    [fMaschine] uniqueidentifier  NOT NULL,
    [fAktivMaschine] uniqueidentifier  NULL
);
GO

-- Creating table 'tabProtokollSet'
CREATE TABLE [dbo].[tabProtokollSet] (
    [Id] uniqueidentifier  NOT NULL,
    [AuswertungStart] datetime  NOT NULL,
    [AuswertungEnde] datetime  NULL,
    [LetzteZeile] int  NOT NULL,
    [LetzteDateiDatum] datetime  NOT NULL,
    [ProtokollText] nvarchar(max)  NULL,
    [Status] tinyint  NOT NULL,
    [AnzahlDurchlauf] tinyint  NOT NULL,
    [FehlerVerbindungMaschine] int  NOT NULL,
    [FehlerPfadZurMaschine] int  NOT NULL,
    [FehlerDatenImport] int  NOT NULL,
    [FehlerDatenSpeichern] int  NOT NULL,
    [DatenAbgleich_Datum] datetime  NOT NULL,
    [DatenAbgleich_Status] tinyint  NOT NULL,
    [DatenAbgleich_Bearbeiter] nvarchar(60)  NOT NULL,
    [DatenAbgleich_Geloescht] bit  NOT NULL
);
GO

-- Creating table 'tabReparaturSet'
CREATE TABLE [dbo].[tabReparaturSet] (
    [Id] uniqueidentifier  NOT NULL,
    [VorgangBeginn] datetime  NOT NULL,
    [VorgangEnde] datetime  NULL,
    [Vorgang] tinyint  NOT NULL,
    [ProtokollText] nvarchar(max)  NULL,
    [CoilwechselAnzahl] tinyint  NULL,
    [DatenAbgleich_Datum] datetime  NOT NULL,
    [DatenAbgleich_Status] tinyint  NOT NULL,
    [DatenAbgleich_Bearbeiter] nvarchar(60)  NOT NULL,
    [DatenAbgleich_Geloescht] bit  NOT NULL,
    [fMaschine] uniqueidentifier  NOT NULL,
    [fVerursacher] uniqueidentifier  NULL,
    [fProtokollant] uniqueidentifier  NULL
);
GO

-- Creating table 'tabStandortSet'
CREATE TABLE [dbo].[tabStandortSet] (
    [Id] uniqueidentifier  NOT NULL,
    [Bezeichnung] nvarchar(120)  NOT NULL,
    [Bemerkung] nvarchar(120)  NULL,
    [DatenAbgleich_Datum] datetime  NOT NULL,
    [DatenAbgleich_Status] tinyint  NOT NULL,
    [DatenAbgleich_Bearbeiter] nvarchar(60)  NOT NULL,
    [DatenAbgleich_Geloescht] bit  NOT NULL
);
GO

-- Creating table 'tabAuswertungSet'
CREATE TABLE [dbo].[tabAuswertungSet] (
    [Id] uniqueidentifier  NOT NULL,
    [ReportName] nvarchar(120)  NOT NULL,
    [ErstelltName] nvarchar(120)  NOT NULL,
    [ErstelltDatum] datetime  NOT NULL,
    [GeaendertName] nvarchar(120)  NOT NULL,
    [GeaendertDatum] datetime  NOT NULL,
    [Report] varbinary(max)  NULL,
    [Bemerkung] nvarchar(max)  NULL,
    [FilterAuswertung] tinyint  NOT NULL,
    [DatenAbgleich_Datum] datetime  NOT NULL,
    [DatenAbgleich_Status] tinyint  NOT NULL,
    [DatenAbgleich_Bearbeiter] nvarchar(60)  NOT NULL,
    [DatenAbgleich_Geloescht] bit  NOT NULL
);
GO

-- Creating table 'tabArbeitszeitSet'
CREATE TABLE [dbo].[tabArbeitszeitSet] (
    [Id] uniqueidentifier  NOT NULL,
    [Anmeldung] datetime  NULL,
    [Abmeldung] datetime  NULL,
    [ManuelleAnmeldung] bit  NOT NULL,
    [ManuelleAbmeldung] bit  NOT NULL,
    [DatenAbgleich_Datum] datetime  NOT NULL,
    [DatenAbgleich_Status] tinyint  NOT NULL,
    [DatenAbgleich_Bearbeiter] nvarchar(60)  NOT NULL,
    [DatenAbgleich_Geloescht] bit  NOT NULL,
    [fBediener] uniqueidentifier  NOT NULL,
    [fStandort] uniqueidentifier  NOT NULL,
    [fArbeitszeitAuswertung] uniqueidentifier  NULL
);
GO

-- Creating table 'tabAnmeldungReparaturSet'
CREATE TABLE [dbo].[tabAnmeldungReparaturSet] (
    [Id] uniqueidentifier  NOT NULL,
    [Anmeldung] datetime  NOT NULL,
    [Abmeldung] datetime  NULL,
    [DatenAbgleich_Datum] datetime  NOT NULL,
    [DatenAbgleich_Status] tinyint  NOT NULL,
    [DatenAbgleich_Bearbeiter] nvarchar(60)  NOT NULL,
    [DatenAbgleich_Geloescht] bit  NOT NULL,
    [fReparatur] uniqueidentifier  NOT NULL,
    [fBediener] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'tabArbeitszeitTagSet'
CREATE TABLE [dbo].[tabArbeitszeitTagSet] (
    [Id] uniqueidentifier  NOT NULL,
    [Tag] tinyint  NOT NULL,
    [Pause] time  NOT NULL,
    [Zeit] time  NOT NULL,
    [NachtschichtZuschlag] time  NOT NULL,
    [FeiertagZuschlag] time  NOT NULL,
    [Urlaub] bit  NOT NULL,
    [Krank] bit  NOT NULL,
    [Feiertag] bit  NOT NULL,
    [Bemerkung] nvarchar(255)  NULL,
    [DatenAbgleich_Datum] datetime  NOT NULL,
    [DatenAbgleich_Status] tinyint  NOT NULL,
    [DatenAbgleich_Bearbeiter] nvarchar(60)  NOT NULL,
    [DatenAbgleich_Geloescht] bit  NOT NULL,
    [fArbeitszeitAuswertung] uniqueidentifier  NOT NULL,
    [IstManuellGeaendert] bit  NOT NULL
);
GO

-- Creating table 'tabFeiertageSet'
CREATE TABLE [dbo].[tabFeiertageSet] (
    [Id] uniqueidentifier  NOT NULL,
    [Datum] datetime  NOT NULL,
    [Bezeichnung] nvarchar(255)  NULL,
    [DatenAbgleich_Datum] datetime  NOT NULL,
    [DatenAbgleich_Status] tinyint  NOT NULL,
    [DatenAbgleich_Bearbeiter] nvarchar(60)  NOT NULL,
    [DatenAbgleich_Geloescht] bit  NOT NULL
);
GO

-- Creating table 'tabArbeitszeitAuswertungSet'
CREATE TABLE [dbo].[tabArbeitszeitAuswertungSet] (
    [Id] uniqueidentifier  NOT NULL,
    [Jahr] smallint  NOT NULL,
    [Monat] tinyint  NOT NULL,
    [SollStunden] nvarchar(7)  NULL,
    [Ueberstunden] nvarchar(7)  NULL,
    [NachtschichtZuschlaege] nvarchar(7)  NULL,
    [FeiertagZuschlaege] nvarchar(7)  NULL,
    [Urlaub] smallint  NOT NULL,
    [Krank] smallint  NOT NULL,
    [Feiertage] smallint  NOT NULL,
    [AuszahlungUeberstunden] nvarchar(7)  NULL,
    [Bemerkung] nvarchar(255)  NULL,
    [Status] tinyint  NOT NULL,
    [DatenAbgleich_Datum] datetime  NOT NULL,
    [DatenAbgleich_Status] tinyint  NOT NULL,
    [DatenAbgleich_Bearbeiter] nvarchar(60)  NOT NULL,
    [DatenAbgleich_Geloescht] bit  NOT NULL,
    [fBediener] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'tabSollStundenSet'
CREATE TABLE [dbo].[tabSollStundenSet] (
    [Id] uniqueidentifier  NOT NULL,
    [Jahr] smallint  NOT NULL,
    [Monat] tinyint  NOT NULL,
    [SollStunden] nvarchar(7)  NULL,
    [DatenAbgleich_Datum] datetime  NOT NULL,
    [DatenAbgleich_Status] tinyint  NOT NULL,
    [DatenAbgleich_Bearbeiter] nvarchar(60)  NOT NULL,
    [DatenAbgleich_Geloescht] bit  NOT NULL
);
GO

-- Creating table 'tabPausenzeitSet'
CREATE TABLE [dbo].[tabPausenzeitSet] (
    [Id] uniqueidentifier  NOT NULL,
    [ZeitVon] time  NOT NULL,
    [ZeitBis] time  NOT NULL,
    [Pausenzeit] time  NOT NULL,
    [DatenAbgleich_Datum] datetime  NOT NULL,
    [DatenAbgleich_Status] tinyint  NOT NULL,
    [DatenAbgleich_Bearbeiter] nvarchar(60)  NOT NULL,
    [DatenAbgleich_Geloescht] bit  NOT NULL
);
GO

-- Creating table 'tabArbeitszeitRundenSet'
CREATE TABLE [dbo].[tabArbeitszeitRundenSet] (
    [Id] uniqueidentifier  NOT NULL,
    [Jahr] smallint  NOT NULL,
    [Monat] tinyint  NOT NULL,
    [ZeitVon] time  NOT NULL,
    [ZeitBis] time  NOT NULL,
    [RundenArbeitszeitBeginn] time  NOT NULL,
    [RundenArbeitszeitLaenge] time  NULL,
    [DatenAbgleich_Datum] datetime  NOT NULL,
    [DatenAbgleich_Status] tinyint  NOT NULL,
    [DatenAbgleich_Bearbeiter] nvarchar(60)  NOT NULL,
    [DatenAbgleich_Geloescht] bit  NOT NULL,
    [fStandort] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'tabArbeitszeitTerminalSet'
CREATE TABLE [dbo].[tabArbeitszeitTerminalSet] (
    [Id] uniqueidentifier  NOT NULL,
    [Bezeichnung] nvarchar(120)  NOT NULL,
    [IpNummer] nvarchar(30)  NOT NULL,
    [PortNummer] int  NOT NULL,
    [UpdateTerminal] bit  NOT NULL,
    [AnzahlFehler] smallint  NOT NULL,
    [DatenAbgleich_Datum] datetime  NOT NULL,
    [DatenAbgleich_Status] tinyint  NOT NULL,
    [DatenAbgleich_Bearbeiter] nvarchar(60)  NOT NULL,
    [DatenAbgleich_Geloescht] bit  NOT NULL,
    [fStandort] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'tabBauteiltabBediener'
CREATE TABLE [dbo].[tabBauteiltabBediener] (
    [sBauteile_Id] uniqueidentifier  NOT NULL,
    [sBediener_Id] uniqueidentifier  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'tabMaschineSet'
ALTER TABLE [dbo].[tabMaschineSet]
ADD CONSTRAINT [PK_tabMaschineSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'tabBedienerSet'
ALTER TABLE [dbo].[tabBedienerSet]
ADD CONSTRAINT [PK_tabBedienerSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'tabBauteilSet'
ALTER TABLE [dbo].[tabBauteilSet]
ADD CONSTRAINT [PK_tabBauteilSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'tabAnmeldungMaschineSet'
ALTER TABLE [dbo].[tabAnmeldungMaschineSet]
ADD CONSTRAINT [PK_tabAnmeldungMaschineSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'tabProtokollSet'
ALTER TABLE [dbo].[tabProtokollSet]
ADD CONSTRAINT [PK_tabProtokollSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'tabReparaturSet'
ALTER TABLE [dbo].[tabReparaturSet]
ADD CONSTRAINT [PK_tabReparaturSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'tabStandortSet'
ALTER TABLE [dbo].[tabStandortSet]
ADD CONSTRAINT [PK_tabStandortSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'tabAuswertungSet'
ALTER TABLE [dbo].[tabAuswertungSet]
ADD CONSTRAINT [PK_tabAuswertungSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'tabArbeitszeitSet'
ALTER TABLE [dbo].[tabArbeitszeitSet]
ADD CONSTRAINT [PK_tabArbeitszeitSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'tabAnmeldungReparaturSet'
ALTER TABLE [dbo].[tabAnmeldungReparaturSet]
ADD CONSTRAINT [PK_tabAnmeldungReparaturSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'tabArbeitszeitTagSet'
ALTER TABLE [dbo].[tabArbeitszeitTagSet]
ADD CONSTRAINT [PK_tabArbeitszeitTagSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'tabFeiertageSet'
ALTER TABLE [dbo].[tabFeiertageSet]
ADD CONSTRAINT [PK_tabFeiertageSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'tabArbeitszeitAuswertungSet'
ALTER TABLE [dbo].[tabArbeitszeitAuswertungSet]
ADD CONSTRAINT [PK_tabArbeitszeitAuswertungSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'tabSollStundenSet'
ALTER TABLE [dbo].[tabSollStundenSet]
ADD CONSTRAINT [PK_tabSollStundenSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'tabPausenzeitSet'
ALTER TABLE [dbo].[tabPausenzeitSet]
ADD CONSTRAINT [PK_tabPausenzeitSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'tabArbeitszeitRundenSet'
ALTER TABLE [dbo].[tabArbeitszeitRundenSet]
ADD CONSTRAINT [PK_tabArbeitszeitRundenSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'tabArbeitszeitTerminalSet'
ALTER TABLE [dbo].[tabArbeitszeitTerminalSet]
ADD CONSTRAINT [PK_tabArbeitszeitTerminalSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [sBauteile_Id], [sBediener_Id] in table 'tabBauteiltabBediener'
ALTER TABLE [dbo].[tabBauteiltabBediener]
ADD CONSTRAINT [PK_tabBauteiltabBediener]
    PRIMARY KEY CLUSTERED ([sBauteile_Id], [sBediener_Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [fMaschine] in table 'tabAnmeldungMaschineSet'
ALTER TABLE [dbo].[tabAnmeldungMaschineSet]
ADD CONSTRAINT [FK_tabMaschinetabAnmeldung]
    FOREIGN KEY ([fMaschine])
    REFERENCES [dbo].[tabMaschineSet]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tabMaschinetabAnmeldung'
CREATE INDEX [IX_FK_tabMaschinetabAnmeldung]
ON [dbo].[tabAnmeldungMaschineSet]
    ([fMaschine]);
GO

-- Creating foreign key on [fBediener] in table 'tabAnmeldungMaschineSet'
ALTER TABLE [dbo].[tabAnmeldungMaschineSet]
ADD CONSTRAINT [FK_tabBedienertabAnmeldung]
    FOREIGN KEY ([fBediener])
    REFERENCES [dbo].[tabBedienerSet]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tabBedienertabAnmeldung'
CREATE INDEX [IX_FK_tabBedienertabAnmeldung]
ON [dbo].[tabAnmeldungMaschineSet]
    ([fBediener]);
GO

-- Creating foreign key on [fVerursacher] in table 'tabReparaturSet'
ALTER TABLE [dbo].[tabReparaturSet]
ADD CONSTRAINT [FK_tabBedienertabReparatur]
    FOREIGN KEY ([fVerursacher])
    REFERENCES [dbo].[tabBedienerSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tabBedienertabReparatur'
CREATE INDEX [IX_FK_tabBedienertabReparatur]
ON [dbo].[tabReparaturSet]
    ([fVerursacher]);
GO

-- Creating foreign key on [fProtokollant] in table 'tabReparaturSet'
ALTER TABLE [dbo].[tabReparaturSet]
ADD CONSTRAINT [FK_tabBedienertabReparatur1]
    FOREIGN KEY ([fProtokollant])
    REFERENCES [dbo].[tabBedienerSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tabBedienertabReparatur1'
CREATE INDEX [IX_FK_tabBedienertabReparatur1]
ON [dbo].[tabReparaturSet]
    ([fProtokollant]);
GO

-- Creating foreign key on [fMaschine] in table 'tabReparaturSet'
ALTER TABLE [dbo].[tabReparaturSet]
ADD CONSTRAINT [FK_tabMaschinetabReparatur]
    FOREIGN KEY ([fMaschine])
    REFERENCES [dbo].[tabMaschineSet]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tabMaschinetabReparatur'
CREATE INDEX [IX_FK_tabMaschinetabReparatur]
ON [dbo].[tabReparaturSet]
    ([fMaschine]);
GO

-- Creating foreign key on [Id] in table 'tabProtokollSet'
ALTER TABLE [dbo].[tabProtokollSet]
ADD CONSTRAINT [FK_tabMaschinetabProtokoll]
    FOREIGN KEY ([Id])
    REFERENCES [dbo].[tabMaschineSet]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [fStandort] in table 'tabMaschineSet'
ALTER TABLE [dbo].[tabMaschineSet]
ADD CONSTRAINT [FK_tabStandorttabMaschine]
    FOREIGN KEY ([fStandort])
    REFERENCES [dbo].[tabStandortSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tabStandorttabMaschine'
CREATE INDEX [IX_FK_tabStandorttabMaschine]
ON [dbo].[tabMaschineSet]
    ([fStandort]);
GO

-- Creating foreign key on [fBediener] in table 'tabArbeitszeitSet'
ALTER TABLE [dbo].[tabArbeitszeitSet]
ADD CONSTRAINT [FK_tabBedienertabArbeitszeit]
    FOREIGN KEY ([fBediener])
    REFERENCES [dbo].[tabBedienerSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tabBedienertabArbeitszeit'
CREATE INDEX [IX_FK_tabBedienertabArbeitszeit]
ON [dbo].[tabArbeitszeitSet]
    ([fBediener]);
GO

-- Creating foreign key on [sBauteile_Id] in table 'tabBauteiltabBediener'
ALTER TABLE [dbo].[tabBauteiltabBediener]
ADD CONSTRAINT [FK_tabBauteiltabBediener_tabBauteil]
    FOREIGN KEY ([sBauteile_Id])
    REFERENCES [dbo].[tabBauteilSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [sBediener_Id] in table 'tabBauteiltabBediener'
ALTER TABLE [dbo].[tabBauteiltabBediener]
ADD CONSTRAINT [FK_tabBauteiltabBediener_tabBediener]
    FOREIGN KEY ([sBediener_Id])
    REFERENCES [dbo].[tabBedienerSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tabBauteiltabBediener_tabBediener'
CREATE INDEX [IX_FK_tabBauteiltabBediener_tabBediener]
ON [dbo].[tabBauteiltabBediener]
    ([sBediener_Id]);
GO

-- Creating foreign key on [fStandort] in table 'tabArbeitszeitSet'
ALTER TABLE [dbo].[tabArbeitszeitSet]
ADD CONSTRAINT [FK_tabStandorttabArbeitszeit1]
    FOREIGN KEY ([fStandort])
    REFERENCES [dbo].[tabStandortSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tabStandorttabArbeitszeit1'
CREATE INDEX [IX_FK_tabStandorttabArbeitszeit1]
ON [dbo].[tabArbeitszeitSet]
    ([fStandort]);
GO

-- Creating foreign key on [fStandort] in table 'tabBedienerSet'
ALTER TABLE [dbo].[tabBedienerSet]
ADD CONSTRAINT [FK_tabStandorttabBediener]
    FOREIGN KEY ([fStandort])
    REFERENCES [dbo].[tabStandortSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tabStandorttabBediener'
CREATE INDEX [IX_FK_tabStandorttabBediener]
ON [dbo].[tabBedienerSet]
    ([fStandort]);
GO

-- Creating foreign key on [fAktivBauteil] in table 'tabMaschineSet'
ALTER TABLE [dbo].[tabMaschineSet]
ADD CONSTRAINT [FK_tabBauteiltabMaschine]
    FOREIGN KEY ([fAktivBauteil])
    REFERENCES [dbo].[tabBauteilSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tabBauteiltabMaschine'
CREATE INDEX [IX_FK_tabBauteiltabMaschine]
ON [dbo].[tabMaschineSet]
    ([fAktivBauteil]);
GO

-- Creating foreign key on [fMaschine] in table 'tabBauteilSet'
ALTER TABLE [dbo].[tabBauteilSet]
ADD CONSTRAINT [FK_tabBauteiltabMaschine1]
    FOREIGN KEY ([fMaschine])
    REFERENCES [dbo].[tabMaschineSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tabBauteiltabMaschine1'
CREATE INDEX [IX_FK_tabBauteiltabMaschine1]
ON [dbo].[tabBauteilSet]
    ([fMaschine]);
GO

-- Creating foreign key on [fReparatur] in table 'tabAnmeldungReparaturSet'
ALTER TABLE [dbo].[tabAnmeldungReparaturSet]
ADD CONSTRAINT [FK_tabReparaturtabAnmeldungReparatur]
    FOREIGN KEY ([fReparatur])
    REFERENCES [dbo].[tabReparaturSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tabReparaturtabAnmeldungReparatur'
CREATE INDEX [IX_FK_tabReparaturtabAnmeldungReparatur]
ON [dbo].[tabAnmeldungReparaturSet]
    ([fReparatur]);
GO

-- Creating foreign key on [fBediener] in table 'tabAnmeldungReparaturSet'
ALTER TABLE [dbo].[tabAnmeldungReparaturSet]
ADD CONSTRAINT [FK_tabBedienertabAnmeldungReparatur]
    FOREIGN KEY ([fBediener])
    REFERENCES [dbo].[tabBedienerSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tabBedienertabAnmeldungReparatur'
CREATE INDEX [IX_FK_tabBedienertabAnmeldungReparatur]
ON [dbo].[tabAnmeldungReparaturSet]
    ([fBediener]);
GO

-- Creating foreign key on [fAktivReparatur] in table 'tabMaschineSet'
ALTER TABLE [dbo].[tabMaschineSet]
ADD CONSTRAINT [FK_tabReparaturtabMaschine]
    FOREIGN KEY ([fAktivReparatur])
    REFERENCES [dbo].[tabReparaturSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tabReparaturtabMaschine'
CREATE INDEX [IX_FK_tabReparaturtabMaschine]
ON [dbo].[tabMaschineSet]
    ([fAktivReparatur]);
GO

-- Creating foreign key on [fAktivArbeitszeit] in table 'tabBedienerSet'
ALTER TABLE [dbo].[tabBedienerSet]
ADD CONSTRAINT [FK_tabArbeitszeittabBediener]
    FOREIGN KEY ([fAktivArbeitszeit])
    REFERENCES [dbo].[tabArbeitszeitSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tabArbeitszeittabBediener'
CREATE INDEX [IX_FK_tabArbeitszeittabBediener]
ON [dbo].[tabBedienerSet]
    ([fAktivArbeitszeit]);
GO

-- Creating foreign key on [fAktivMaschine] in table 'tabAnmeldungMaschineSet'
ALTER TABLE [dbo].[tabAnmeldungMaschineSet]
ADD CONSTRAINT [FK_tabMaschinetabAnmeldungMaschine]
    FOREIGN KEY ([fAktivMaschine])
    REFERENCES [dbo].[tabMaschineSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tabMaschinetabAnmeldungMaschine'
CREATE INDEX [IX_FK_tabMaschinetabAnmeldungMaschine]
ON [dbo].[tabAnmeldungMaschineSet]
    ([fAktivMaschine]);
GO

-- Creating foreign key on [fArbeitszeitAuswertung] in table 'tabArbeitszeitSet'
ALTER TABLE [dbo].[tabArbeitszeitSet]
ADD CONSTRAINT [FK_tabArbeitszeitAuswertungtabArbeitszeit]
    FOREIGN KEY ([fArbeitszeitAuswertung])
    REFERENCES [dbo].[tabArbeitszeitTagSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tabArbeitszeitAuswertungtabArbeitszeit'
CREATE INDEX [IX_FK_tabArbeitszeitAuswertungtabArbeitszeit]
ON [dbo].[tabArbeitszeitSet]
    ([fArbeitszeitAuswertung]);
GO

-- Creating foreign key on [fBediener] in table 'tabArbeitszeitAuswertungSet'
ALTER TABLE [dbo].[tabArbeitszeitAuswertungSet]
ADD CONSTRAINT [FK_tabBedienertabArbeitszeitAuswertung]
    FOREIGN KEY ([fBediener])
    REFERENCES [dbo].[tabBedienerSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tabBedienertabArbeitszeitAuswertung'
CREATE INDEX [IX_FK_tabBedienertabArbeitszeitAuswertung]
ON [dbo].[tabArbeitszeitAuswertungSet]
    ([fBediener]);
GO

-- Creating foreign key on [fArbeitszeitAuswertung] in table 'tabArbeitszeitTagSet'
ALTER TABLE [dbo].[tabArbeitszeitTagSet]
ADD CONSTRAINT [FK_tabArbeitszeitAuswertungtabArbeitszeitTag]
    FOREIGN KEY ([fArbeitszeitAuswertung])
    REFERENCES [dbo].[tabArbeitszeitAuswertungSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tabArbeitszeitAuswertungtabArbeitszeitTag'
CREATE INDEX [IX_FK_tabArbeitszeitAuswertungtabArbeitszeitTag]
ON [dbo].[tabArbeitszeitTagSet]
    ([fArbeitszeitAuswertung]);
GO

-- Creating foreign key on [fStandort] in table 'tabArbeitszeitRundenSet'
ALTER TABLE [dbo].[tabArbeitszeitRundenSet]
ADD CONSTRAINT [FK_tabStandorttabArbeitzzeitRunden]
    FOREIGN KEY ([fStandort])
    REFERENCES [dbo].[tabStandortSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tabStandorttabArbeitzzeitRunden'
CREATE INDEX [IX_FK_tabStandorttabArbeitzzeitRunden]
ON [dbo].[tabArbeitszeitRundenSet]
    ([fStandort]);
GO

-- Creating foreign key on [fStandort] in table 'tabArbeitszeitTerminalSet'
ALTER TABLE [dbo].[tabArbeitszeitTerminalSet]
ADD CONSTRAINT [FK_tabStandorttabArbeitszeitTerminal]
    FOREIGN KEY ([fStandort])
    REFERENCES [dbo].[tabStandortSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tabStandorttabArbeitszeitTerminal'
CREATE INDEX [IX_FK_tabStandorttabArbeitszeitTerminal]
ON [dbo].[tabArbeitszeitTerminalSet]
    ([fStandort]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------