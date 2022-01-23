Quarter is a time-tracker for personal use with the goal to make it as effortless as possible
to track your time.

- Graphical timesheet for easy tracking
- Simple reports for basic needs
- 100% Open HTTP API for everything else

![quarter-wide](https://user-images.githubusercontent.com/136971/150692080-97a16190-2172-42c4-87ff-e63ff55e8ba4.png)

## Current state

**Under construction - Quarter is under construction and not yet ready for daily use just yet.**

- [x] Authentication
- [x] Manage projects
- [x] Manage activities
- [x] Register time
- [x] Summarize current week at start page
- [ ] Weekly report
- [ ] Archive projects and activities
- [ ] HTTP API
- [ ] Export data

## Getting started

TL;DR
```
git clone git@github.com:eliasson/quarter.git
cd quarter
docker-compose -f docker/docker-compose.yaml up
dotnet build
dotnet run --project src/Quarter 
```

See [getting-started.md](docs/getting-started.md) for more details.

## What features will it support

The prime use-case for Quarter is to offer a convenient way to input hours on a daily basis. It should
also be possible to render reports that aggregates per day or week. All other features are second to that.

- Input time using the unit quarters of an hour
- Aggregate time per day and week
- User have full access to all its data via API and data export
- Run hosted for multiple users

What will it not support:

- There will be no timer, time is input manually
- There will be no billing support
- There will be no teams, projects cannot be shared and timesheets will not be submitted

## Why

Quarter was born out of the frustration of using two or three different time reporting tools
simultaneously (being a consultant). It was obvious that most of these systems were made to
extract hours (reports and invoices), not input hours.

So Quarter was a simple way to keep track of time spent on different projects on a daily
basis. Then at the end of the week transcribe those hours into other time tracking tools.

This is one of many rewrites, there is a version written in Python and Django from 2014
still in use at [quarterapp.com](http://www.quarterapp.com). This is aimed to take its place once ready.

_The former name used to be QuarterApp, that name and its abbreviation QA is still around in a few places._

## Design principles

- As few dependencies as possible
- Keep code size small
- Deploy anywhere (no SAAS / FAAS / Cloud lock-in!)
- No tracking

But above all - working on Quarter should be fun! After all, it is open source and the people
building it are the same people that is using it.

## Contributing

Contributions are welcome! If you want to contribute, open an issue or comment on an existing
one and we'll take it from there.

For any contribution, the following applies:

- An issue must be opened and all features needs to be discussed
- Tests must be added, if relevant
- Documentation must be added, if relevant
- In the absence of style guidelines, please stick to the existing style

## License

Released under GNU Affero General Public License v3.0, see LICENCE for details.
