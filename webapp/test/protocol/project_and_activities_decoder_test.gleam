import gleam/json
import gleam/list
import gleeunit/should
import project
import protocol

// Test that parse project and activities to ensure that activities are added to the
// correct projects.
//
// TODO Any orphan project should be ignored.

pub fn inflates_to_empty_list_of_projects_test() {
  "{
    \"projects\": [],
    \"activities\": []
  }"
  |> json.parse(protocol.project_and_activities_decoder())
  |> should.be_ok()
  |> should.equal([])
}

pub fn inflates_to_child_less_projects_test() {
  "{
     \"projects\": [{
        \"id\": \"P01\",
        \"name\": \"Project Alpha\",
        \"description\": \"The alpha project\",
        \"isArchived\": false,
        \"created\": \"2025-11-04T16:49:39.2993437Z\"
     },{
        \"id\": \"P02\",
        \"name\": \"Project Bravo\",
        \"description\": \"The bravo project\",
        \"isArchived\": false,
        \"created\": \"2025-11-04T16:49:39.2993437Z\"
    }],
    \"activities\": [{
      \"id\": \"A01\",
      \"projectId\": \"P01\",
      \"name\": \"Activity Alpha\",
      \"description\": \"The alpha activity\",
      \"color\": \"#8E87F5\",
      \"isArchived\": false,
      \"created\": \"2025-11-04T16:49:39.2993437Z\"
    },{
      \"id\": \"A02\",
      \"projectId\": \"P01\",
      \"name\": \"Activity Bravo\",
      \"description\": \"The bravo activity\",
      \"color\": \"#8E87F5\",
      \"isArchived\": false,
      \"created\": \"2025-11-04T16:49:39.2993437Z\"
    },{
      \"id\": \"A03\",
      \"projectId\": \"P02\",
      \"name\": \"Activity Charlie\",
      \"description\": \"The charlie activity\",
      \"color\": \"#8E87F5\",
      \"isArchived\": false,
      \"created\": \"2025-11-04T16:49:39.2993437Z\"
    }]
  }"
  |> json.parse(protocol.project_and_activities_decoder())
  |> should.be_ok()
  |> expect_project_with_activities([
    #("P01", ["A01", "A02"]),
    #("P02", ["A03"]),
  ])
}

fn expect_project_with_activities(
  projects: List(project.Project),
  expected: List(#(String, List(String))),
) {
  let actual =
    list.map(projects, fn(p) {
      #(p.id.value, list.map(p.activities, fn(a) { a.id.value }))
    })
  is_subset(actual, expected)
  |> should.be_true
}

fn is_subset(super: List(a), sub: List(a)) -> Bool {
  list.all(sub, fn(e) { list.contains(super, e) })
}
