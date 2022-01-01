USE Accounts;
GO

CREATE TABLE [dbo].[t_Accounts](
  account_id INT NOT NULL,
  account_name varchar(250) NOT NULL,
  is_active bit NOT NULL,
  PRIMARY KEY (account_id)
);
GO