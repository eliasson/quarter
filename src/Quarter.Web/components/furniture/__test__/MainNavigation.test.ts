import { beforeEach, describe, expect, it } from "vitest"
import { shallowMount, VueWrapper } from "@vue/test-utils"
import DropDownMenu from "@/components/DropDownMenu.vue"
import MainNavigation from "@/components/furniture/MainNavigation.vue"
import { AppPaths } from "@/router"

describe("<main-navigation>", () => {
    let wrapper: VueWrapper

    beforeEach(() => {
        wrapper = mountComponent()
    })

    describe("initially", () => {
        it("should have application menu", () => {
            expect(ui().applicationMenu()?.props()).toMatchObject({
                triggerIcon: "icon-menu",
                triggerTitle: "Main menu",
            })
        })

        it("should have expected application menu items", () => {
            const actual = ui().applicationMenu()?.props().items.map(i =>
                [i.title, i.subTitle, i.icon, i.link])

            expect(actual).toEqual([
                ["Timesheets", "Register activity for a day.", "icon-timesheet", AppPaths.Timesheets],
                ["Projects", "Manage your projects and activities.", "icon-info", AppPaths.Projects],
                ["Users", "Manage registered users.", "icon-user", AppPaths.Users],
            ])
        })
    })

    function ui() {
        return {
            applicationMenu: () => wrapper.findAllComponents(DropDownMenu).at(0),
        }
    }

    function mountComponent() {
        return shallowMount(MainNavigation, { })
    }
})
