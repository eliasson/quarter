import { describe, it, expect, beforeEach } from "vitest"
import { ApiClient, type IApiClient, type UserResourceOutput } from "@/utils/api-client.ts"
import { User } from "@/models/user.ts"
import { FakeHttpClient } from "@/utils/test-utils/fake-http-client.ts"
import { successful } from "@/utils/test-utils.ts"

describe("ApiClient", () => {
    let client: IApiClient
    let httpClient: FakeHttpClient

    beforeEach(() => {
        httpClient = new FakeHttpClient()
        client = new ApiClient(httpClient)
    })

    describe("getCurrentUser", () => {
        beforeEach(() => {
            const userResource = {
                id: "user-123",
                email: "jane.doe@example.com",
                created: "2025-03-26T19:30:24.0939490Z",
                updated: null,
            }
            httpClient.getResponses.set("/api/users/self", successful<UserResourceOutput>(userResource))
        })

        it("should GET user from /user/self", async () => {
            const user = await client.getCurrentUser()

            expect(user).toEqual(new User("user-123", "jane.doe@example.com"))
        })
    })
})
