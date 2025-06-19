/** Throws an error with the given message. Useful to throw errors as an expression. */
export function raiseError(message: string): never {
    throw new Error(message)
}

export class ControllablePromise<T>  {
    promise: Promise<T>
    private resolve?: (value: (PromiseLike<T> | T)) => void;
    private reject?: (reason?: any) => void;

    constructor() {
        this.promise = new Promise<T>((resolve, reject) => {
            this.resolve = resolve;
            this.reject = reject;
        });
    }

    resolveWith(value: T): void {
        this.resolve?.(value)
    }

    rejectWith(value: T): void {
        this.reject?.(value)
    }
}
