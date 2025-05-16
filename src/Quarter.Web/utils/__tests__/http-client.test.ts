import { beforeEach, describe, expect, it, vi, type Mock } from "vitest"
import { type FetchFunction, HttpClient, HttpError } from "@/utils/http-client.ts"
import { fakeResponse } from "@/utils/test-utils/test-utils.ts"

/** The Content-Type header returned by quarter backend for JSON responses. */
const JsonContentType = "application/json; charset=utf-8";

describe("HttpClient", () => {
    let client: HttpClient
    let fetcher: Mock<FetchFunction>

    beforeEach(() => {
        fetcher = vi.fn()
        client = new HttpClient(fetcher)
    })

    describe("get", () => {
        describe("when successful", () => {
            beforeEach(() => {
                fetcher.mockReturnValue(Promise.resolve(fakeResponse({
                    status: 200,
                    body: `{"foo":"bar"}`,
                    headers: {
                        "content-type": JsonContentType,
                    }
                })))
                void client.get<void>("http://localhost/foo")
            })

            it("should call GET", () => {
                expect(fetcher).toHaveBeenCalledWith("http://localhost/foo", {
                    method: "GET",
                })
            })
        })

        describe("when response is not 200 OK", () => {
            beforeEach(() => {
                fetcher.mockReturnValue(Promise.resolve(fakeResponse({
                    status: 404,
                    statusText: "Not found"
                })))
            })

            it("should throw a HttpError", async () => {
              await expect( () => client.get<void>("http://localhost/foo")).rejects
                  .toThrow(new HttpError(404, "Not found"))
            })
        })

        describe("when response is not JSON", () => {
            beforeEach(() => {
                fetcher.mockReturnValue(Promise.resolve(fakeResponse({
                    status: 200,
                    headers: {
                        "content-type": "text/plain"
                    },
                })))
            })

            it("should throw an error", async () => {
                await expect( () => client.get<void>("http://localhost/foo")).rejects.toThrowError(new Error("Not a JSON response"))
            })
        })
    })
})
