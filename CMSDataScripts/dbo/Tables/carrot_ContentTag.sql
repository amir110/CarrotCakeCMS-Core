﻿CREATE TABLE [dbo].[carrot_ContentTag] (
    [ContentTagID] UNIQUEIDENTIFIER DEFAULT (newid()) NOT NULL,
    [SiteID]       UNIQUEIDENTIFIER NOT NULL,
    [TagText]      NVARCHAR (256)   NOT NULL,
    [TagSlug]      NVARCHAR (256)   NOT NULL,
    [IsPublic]     BIT              NOT NULL,
    CONSTRAINT [PK_carrot_ContentTag] PRIMARY KEY NONCLUSTERED ([ContentTagID] ASC),
    CONSTRAINT [FK_carrot_ContentTag_SiteID] FOREIGN KEY ([SiteID]) REFERENCES [dbo].[carrot_Sites] ([SiteID])
);


GO
CREATE NONCLUSTERED INDEX [IX_carrot_ContentTag_SiteID]
    ON [dbo].[carrot_ContentTag]([SiteID] ASC);

