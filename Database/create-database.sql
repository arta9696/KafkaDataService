CREATE TABLE "Posts" (
    "Id" INT NOT NULL PRIMARY KEY,
    "Title" VARCHAR(255) NOT NULL,
    "Body" TEXT,
    "Created" DATE,
    "Updated" DATE,
    "Status" INT
);

CREATE TABLE "Comments" (
    "Id" INT NOT NULL PRIMARY KEY,
    "Author" VARCHAR(255) NOT NULL,
    "Description" TEXT,
    "Created" DATE,
    "Updated" DATE,
    "Status" INT,
    "StatusMessage" VARCHAR(255),
    "PostId" INT NOT NULL,
    FOREIGN KEY ("PostId") REFERENCES "Posts"("Id") ON DELETE CASCADE ON UPDATE CASCADE
);