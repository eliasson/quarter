<template>
    <nav class="q-main-navigation">
        <div class="q-main-navigation--item">
            <button type="button" class="q-button q-button__round" title="Main menu">
                <svg class="q-icon">
                    <use xlink:href="#icon-menu"></use>
                </svg>
            </button>
            <!-- div class="q-drop-down-menu">

                <div class="q-menu-item">
                    <RouterLink to="/">
                        <div class="q-menu-item--icon"></div>
                        <div class="q-menu-item--title">Timesheet</div>
                        <div class="q-menu-item--sub-title">Register activity for a day.</div>
                    </RouterLink>
                </div>

                <div class="q-menu-item">
                    <RouterLink to="/admin/users">
                        <div class="q-menu-item--icon"></div>
                        <div class="q-menu-item--title">Users</div>
                        <div class="q-menu-item--sub-title">Manage registered users.</div>
                    </RouterLink>
                </div>

                <div class="q-menu-item">
                    <RouterLink to="/about">
                        <div class="q-menu-item--icon"></div>
                        <div class="q-menu-item--title">About</div>
                        <div class="q-menu-item--sub-title">Test page</div>
                    </RouterLink>
                </div>
            </div -->
        </div>
        <div class="q-main-navigation--item">
            <button type="button" class="q-button q-button__round" title="Main menu">
                <svg class="q-icon">
                    <use xlink:href="#icon-user"></use>
                </svg>
            </button>
        </div>
    </nav>
</template>

<script setup lang="ts">
import {RouterLink} from "vue-router";
</script>

<style scoped>
    .q-main-navigation {
        display: flex;
        justify-content:  space-between;
        padding: 1rem;
    }

    .q-main-navigation--item:first-of-type {
        justify-self: start;
    }

    .q-main-navigation--item:last-of-type {
        justify-self: end;
    }

</style>
