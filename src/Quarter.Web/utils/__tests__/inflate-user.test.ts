import { describe, it, expect, beforeEach } from "vitest"
import { inflateUser } from "@/utils/api-client.ts"
import type { User } from "@/models/user.ts"

describe("inflateUser", () => {
    describe("with minimal data", ( ) => {
        let user: User

        beforeEach(() => user = inflateUser({
            id: "u-123",
            email: "jane.doe@example.com",
            created: "2025-03-26T19:30:24.0939490Z",
            updated: null,
        }))

        it("should map id", () => {
            expect(user.id).toEqual("u-123")
        })

        it("should map email", () => {
            expect(user.email).toEqual("jane.doe@example.com")
        })
    })
})
