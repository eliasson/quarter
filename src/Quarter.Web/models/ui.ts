export type Icon = "icon-timesheet" | "icon-user"

export class MenuItem {
    constructor(
        public title: string,
        public subTitle: string,
        public icon: Icon) { }

    href(): string {
        return `#${this.icon}`;
    }
}
