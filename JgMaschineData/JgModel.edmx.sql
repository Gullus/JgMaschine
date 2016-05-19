
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 05/17/2016 10:13:24
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

IF OBJECT_ID(N'[dbo].[FK_tabMaschinetabDaten]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tabBauteilSet] DROP CONSTRAINT [FK_tabMaschinetabDaten];
GO
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
IF OBJECT_ID(N'[dbo].[FK_tabMaschinetabBediener]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tabBedienerSet] DROP CONSTRAINT [FK_tabMaschinetabBediener];
GO
IF OBJECT_ID(N'[dbo].[FK_tabStandorttabArbeitszeit1]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[tabArbeitszeitSet] DROP CONSTRAINT [FK_tabStandorttabArbeitszeit1];
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
    [ProtokollName] tinyint  NOT NULL,
    [ComputerAdresse] nvarchar(max)  NULL,
    [PfadDaten] nvarchar(max)  NULL,
    [PfadBediener] nvarchar(max)  NULL,
    [ScannerNummer] nvarchar(max)  NULL,
    [ScannerMitDisplay] bit  NOT NULL,
    [Bemerkung] nvarchar(max)  NULL,
    [Status] tinyint  NOT NULL,
    [VorgabeProStunde] decimal(8,3)  NOT NULL,
    [DatenAbgleich_Datum] datetime  NOT NULL,
    [DatenAbgleich_Status] int  NOT NULL,
    [DatenAbgleich_Bearbeiter] nvarchar(max)  NOT NULL,
    [fStandort] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'tabBedienerSet'
CREATE TABLE [dbo].[tabBedienerSet] (
    [Id] uniqueidentifier  NOT NULL,
    [NachName] nvarchar(120)  NOT NULL,
    [VorName] nvarchar(120)  NOT NULL,
    [Bemerkung] nvarchar(max)  NULL,
    [MatchCode] nvarchar(120)  NULL,
    [Status] tinyint  NOT NULL,
    [DatenAbgleich_Datum] datetime  NOT NULL,
    [DatenAbgleich_Status] int  NOT NULL,
    [DatenAbgleich_Bearbeiter] nvarchar(max)  NOT NULL,
    [fAktuellAngemeldet] uniqueidentifier  NULL,
    [fStandort] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'tabBauteilSet'
CREATE TABLE [dbo].[tabBauteilSet] (
    [Id] uniqueidentifier  NOT NULL,
    [DatumStart] datetime  NOT NULL,
    [DatumEnde] datetime  NOT NULL,
    [IdStahlPosition] int  NOT NULL,
    [IdStahlBauteil] int  NOT NULL,
    [BtAnzahl] int  NOT NULL,
    [BtLaenge] int  NOT NULL,
    [BtGewicht] int  NOT NULL,
    [BtDurchmesser] int  NOT NULL,
    [Kunde] nvarchar(120)  NULL,
    [Auftrag] nvarchar(120)  NULL,
    [NummerBauteil] nvarchar(10)  NULL,
    [NummerPosition] nvarchar(10)  NULL,
    [Buegelname] nvarchar(120)  NULL,
    [IstHandeingabe] bit  NOT NULL,
    [AnzahlBediener] tinyint  NOT NULL,
    [AnzahlBiegungen] tinyint  NOT NULL,
    [DatenAbgleich_Datum] datetime  NOT NULL,
    [DatenAbgleich_Status] int  NOT NULL,
    [DatenAbgleich_Bearbeiter] nvarchar(max)  NOT NULL,
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
    [IstAktiv] bit  NOT NULL,
    [DatenAbgleich_Datum] datetime  NOT NULL,
    [DatenAbgleich_Status] int  NOT NULL,
    [DatenAbgleich_Bearbeiter] nvarchar(max)  NOT NULL,
    [fBediener] uniqueidentifier  NOT NULL,
    [fMaschine] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'tabProtokollSet'
CREATE TABLE [dbo].[tabProtokollSet] (
    [Id] uniqueidentifier  NOT NULL,
    [AuswertungStart] datetime  NOT NULL,
    [AuswertungEnde] datetime  NOT NULL,
    [LetztePositionDatum] datetime  NOT NULL,
    [LetzteDateiDatum] datetime  NOT NULL,
    [ProtokollText] nvarchar(max)  NULL,
    [Status] tinyint  NOT NULL,
    [AnzahlDurchlauf] tinyint  NOT NULL,
    [FehlerVerbindungMaschine] int  NOT NULL,
    [FehlerPfadZurMaschine] int  NOT NULL,
    [FehlerDatenImport] int  NOT NULL,
    [FehlerDatenSpeichern] int  NOT NULL,
    [DatenAbgleich_Datum] datetime  NOT NULL,
    [DatenAbgleich_Status] int  NOT NULL,
    [DatenAbgleich_Bearbeiter] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'tabReparaturSet'
CREATE TABLE [dbo].[tabReparaturSet] (
    [Id] uniqueidentifier  NOT NULL,
    [VorgangBeginn] datetime  NOT NULL,
    [VorgangEnde] datetime  NOT NULL,
    [ProtokollText] nvarchar(max)  NULL,
    [Ereigniss] tinyint  NOT NULL,
    [IstAktiv] bit  NOT NULL,
    [DatenAbgleich_Datum] datetime  NOT NULL,
    [DatenAbgleich_Status] int  NOT NULL,
    [DatenAbgleich_Bearbeiter] nvarchar(max)  NOT NULL,
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
    [DatenAbgleich_Status] int  NOT NULL,
    [DatenAbgleich_Bearbeiter] nvarchar(max)  NOT NULL
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
    [DatenAbgleich_Status] int  NOT NULL,
    [DatenAbgleich_Bearbeiter] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'tabArbeitszeitSet'
CREATE TABLE [dbo].[tabArbeitszeitSet] (
    [Id] uniqueidentifier  NOT NULL,
    [Anmeldung] datetime  NOT NULL,
    [Abmeldung] datetime  NOT NULL,
    [ManuelleAnmeldung] bit  NOT NULL,
    [ManuelleAbmeldung] bit  NOT NULL,
    [IstAktiv] bit  NOT NULL,
    [DatenAbgleich_Datum] datetime  NOT NULL,
    [DatenAbgleich_Status] int  NOT NULL,
    [DatenAbgleich_Bearbeiter] nvarchar(max)  NOT NULL,
    [fBediener] uniqueidentifier  NOT NULL,
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

-- Creating primary key on [sBauteile_Id], [sBediener_Id] in table 'tabBauteiltabBediener'
ALTER TABLE [dbo].[tabBauteiltabBediener]
ADD CONSTRAINT [PK_tabBauteiltabBediener]
    PRIMARY KEY CLUSTERED ([sBauteile_Id], [sBediener_Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [fMaschine] in table 'tabBauteilSet'
ALTER TABLE [dbo].[tabBauteilSet]
ADD CONSTRAINT [FK_tabMaschinetabDaten]
    FOREIGN KEY ([fMaschine])
    REFERENCES [dbo].[tabMaschineSet]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tabMaschinetabDaten'
CREATE INDEX [IX_FK_tabMaschinetabDaten]
ON [dbo].[tabBauteilSet]
    ([fMaschine]);
GO

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

-- Creating foreign key on [fAktuellAngemeldet] in table 'tabBedienerSet'
ALTER TABLE [dbo].[tabBedienerSet]
ADD CONSTRAINT [FK_tabMaschinetabBediener]
    FOREIGN KEY ([fAktuellAngemeldet])
    REFERENCES [dbo].[tabMaschineSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_tabMaschinetabBediener'
CREATE INDEX [IX_FK_tabMaschinetabBediener]
ON [dbo].[tabBedienerSet]
    ([fAktuellAngemeldet]);
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

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------