CREATE TABLE {0} (
	ID             integer PRIMARY KEY AUTOINCREMENT NOT NULL,
	Type		   int NOT NULL,
    Name           varchar(30) NOT NULL UNIQUE,
	URL			   varchar(255) NOT NULL UNIQUE,
	ModifiedDate   datetime NOT NULL
)