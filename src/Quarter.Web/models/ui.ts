export type Icon = "icon-menu" | "icon-user" | "icon-timesheet" | "icon-users" | "icon-info"

export class MenuItem {
    constructor(
        public title: string,
        public subTitle: string,
        public icon: Icon) { }

    href(): string {
        return `#${this.icon}`;
    }
}
