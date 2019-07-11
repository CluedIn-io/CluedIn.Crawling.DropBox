USE OpenCommunication
GO
INSERT [dbo].[Provider] ([ID], [Name], [Active],[Plan], [Details], [Type], [Logo], [PullLink], [Category], [Configuration])
VALUES ('203284c2-23a8-4bd5-a73d-252b89461317', N'DropBox', 1, 1, N'Our DropBox provider will allow you to search across all your DropBox accounts.', N'cloud', N'http://immense-refuge-3500.herokuapp.com/img/providers/salesforce.png', N'http://proget.cerebro.technology/salesforce', 'Files', '{ "actions": [ { "name" : "start", "action": "javascript function"}, { "name" : "share", "action": "javascript function for share"} ] }')
GO
