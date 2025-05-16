// TODO: This file should be excluded from the ordinary application and only available for the test code

export type FakResponseOptions = {
    /** Optional body data (since not all status codes require body). */
    body?: string
    status: number,
    statusText?: string
    headers?: { [key: string]: string }
}

/**
 * Create a fake Response object used to test the HTTP client.
 * Only supports what is currently needed by the tests.
 *
 * @param options the properties to use for the response object.
 */
export function fakeResponse(options: FakResponseOptions): Response {
    const fake = {
        status: options.status,
        statusText: options.statusText ??  "",
        ok: options.status >= 200 && options.status < 300,

        /** [MDN Reference](https://developer.mozilla.org/docs/Web/API/Headers) */
        headers: {
            get: (name: string): string | null => {
                return options.headers?.[name] ?? null
            },
            has: (name: string): boolean => {
                return Boolean(options.headers?.[name])
            }
        } as Headers,
        json: () => Promise.resolve(JSON.parse(options.body ?? ""))
    }

    return fake as any as Response
}
