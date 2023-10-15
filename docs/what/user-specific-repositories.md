# User specific repositories

What it is this?

Early on in the project there was this idea that each user (tenant) should have a separate database (e.g. SQLite). The
reason was tenant isolation, possibility to download your own data as SQL, and a simple and safe "forget-about-me"
implementation.

It never took of for three main reasons:

- Migrations would have to be applied to N databases and guarantee to either migrate all or none.
- System data (e.g. users) would need to be stored separately and work would be needed to split the repository factory.
- It is beneficial to be able to query global data for statistics (count total number of hours tracked, etc). 


## Limitations

Until the user specific repositories is removed it is not possible to query projects, timesheets, etc. across users.
This stops us from displaying other metrics than the total number of users in the admin view currently.