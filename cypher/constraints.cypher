CREATE CONSTRAINT user_email IF NOT EXISTS
FOR (user:User) REQUIRE user.email IS UNIQUE;

CREATE CONSTRAINT paper_title IF NOT EXISTS
FOR (paper:Paper) REQUIRE paper.title IS UNIQUE;


CREATE CONSTRAINT paper_url IF NOT EXISTS
FOR (paper:Paper) REQUIRE paper.url IS UNIQUE;
