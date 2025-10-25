import route.{type Route}

pub type Msg {
  Incr
  Decr
  OnRouteChange(Route)
}
