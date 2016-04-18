-- =============================================
-- Author:		Jörg Gullus
-- Create date: 01.02.2015
-- Description:	
-- =============================================

USE [JgMaschine]

IF OBJECT_ID ( 'dbo.BauteilInDaten', 'P' ) IS NOT NULL 
    DROP PROCEDURE dbo.BauteilInDaten;
GO

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[BauteilInDaten]
	@Datum datetime = '01.01.1900', 
	@IdPosition Guid = '1C2F5F4D-4279-4CD3-866C-26AB9F3B3197'
	@IdMaschine Guid = '00000000-0000-0000-0000-000000000000' 
AS
BEGIN
	
	SET NOCOUNT ON;

	DECLARE @AnzahlDatenVorhanden int
	
	-- Kontrolle on Daten mit Bauteilnummer bereits vorhanden

    SELECT TOP 1 @AnzahlDatenVorhanden = COUNT(*) 
		FROM dbo.tabDatenSet AS daten
		WHERE (daten.IdStahlPosition = @IdPosition) AND (daten.IstHandeingabe = 1)

	IF @AnzahlDatenVorhanden > 0
	BEGIN
		SELECT -1     --> Bauteil bereits vorhanden
	END
	ELSE 
	BEGIN

		-- Bauteil in JgData tabBauteil suchen

		SELECT TOP 1 @AnzahlDatenVorhanden = COUNT(*)
			FROM JgData_Schwepnitz.dbo.tabStahlPosition
			WHERE (ECO_ID = @IdPosition)

		IF @AnzahlDatenVorhanden = 0
		BEGIN
			SELECT -2    --> Position wurde nicht gefunden
		END
		ELSE
		BEGIN
			DECLARE @TabRueck TABLE (NeuId int)
			DECLARE @IdDatenRueck bigint

			-- Datensatz mit Daten aus aus JgData erstellen

			INSERT INTO dbo.tabDatenSet (Datum, IdStahlPosition, IdStahlBauteil, NummerPosition, NummerBauteil, IstHandeingabe, BtStueck, BtLaenge, BtGewicht, fMaschine)
			OUTPUT INSERTED.Id INTO @TabRueck

			SELECT TOP 1 @Datum, @IdPosition, stahlPosition.tabStahlPositiontabStahlBauteil_xStahlBauteil, stahlPosition.PositionsNummer, stahlBauteil.NummerBauteil, 1,
					CAST(stahlPosition.AnzahlTeile AS smallint), CAST(stahlPosition.Laenge AS smallint), CAST(stahlPosition.Gewicht AS decimal(8, 3)),
					@IdMaschine
				FROM JgData_Schwepnitz.dbo.tabStahlBauteil AS stahlBauteil INNER JOIN JgData_Schwepnitz.dbo.tabStahlPosition AS stahlPosition 
					ON stahlBauteil.ECO_ID = stahlPosition.tabStahlPositiontabStahlBauteil_xStahlBauteil
				WHERE stahlPosition.ECO_ID = @IdPosition

			-- Aktuelle Id von Daten zurückschicken

			SELECT TOP 1 NeuId FROM @TabRueck
		END 
	END
END