import { describe, it, expect, beforeEach } from "vitest"
import { mount, VueWrapper } from "@vue/test-utils"
import DropDownMenu from "@/components/DropDownMenu.vue"
import { DropDownMenuViewObject } from "@/utils/test-utils/view-models/DropDownMenuViewObject.ts"
import { MenuItem } from "@/models/ui.ts"
import { createRouter, createWebHistory } from "vue-router"
import EmptyComponent from "@/utils/test-utils/EmptyComponent.vue"

describe("<drop-down-menu>", () => {
    let wrapper: VueWrapper
    let router: any

    beforeEach(async () => {
        router = createRouter({
            history: createWebHistory(),
            routes: [{
                path: "/",
                name: "root",
                component: EmptyComponent
            },{
                path: "/alpha",
                name: "alpha",
                component: EmptyComponent
            },{
                path: "/bravo",
                name: "bravo",
                component: EmptyComponent
            }]
        })

        router.push('/')
        await router.isReady()
    })

    beforeEach(() => {
        wrapper = mountComponent({
            items: [
                new MenuItem("Alpha", "The alpha option", "icon-user", "/alpha"),
                new MenuItem("Bravo", "The bravo option", "icon-timesheet", "/bravo"),
            ]
        })
    })

    describe("initially", () => {
        it("should not rendered the menu", () => {
            expect(ui().menu().modal().exists()).toBe(false)
        })

        it("should render the trigger slot", () => {
            const trigger = ui().menu().trigger()
            expect(trigger.exists()).toBe(true)
        })

        describe("when triggered", () => {
            beforeEach(() => ui().menu().trigger().click())

            it("should rendered the menu", () => {
                expect(ui().menu().modal().exists()).toBe(true)
            })

            it("should render the menu items", () => {
                const items = ui().menu().items().map(i =>
                    [i.title(), i.subTitle(), i.icon().icon(), i.link()])

                expect(items).toEqual([
                    ["Alpha", "The alpha option", "#icon-user", "/alpha"],
                    ["Bravo", "The bravo option", "#icon-timesheet", "/bravo"],
                ])
            })
        })
    })

    function ui() {
        const menu = new DropDownMenuViewObject(wrapper)
        return {
            menu: () => menu,
        }
    }

    type TestOptions = { items: MenuItem[] }

    function mountComponent(options: TestOptions) {
        const props = {
            triggerIcon: "unit-test",
            triggerTitle: "TRIGGER",
            items: options.items,
        }

        const plugins = [router]
        return mount(DropDownMenu, { props, global: { plugins } })
    }
})
