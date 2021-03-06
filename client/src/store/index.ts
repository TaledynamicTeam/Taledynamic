import { InjectionKey } from "vue";
import { createStore, useStore as baseUseStore, Store } from "vuex";
import { State } from "@/models/store";
import { state } from "@/store/state";
import { mutations } from "@/store/mutations";
import user from "@/store/modules/user";
import workspaces from "@/store/modules/workspaces";
import table from "@/store/modules/table";

export const key: InjectionKey<Store<State>> = Symbol();

export const store = createStore({
  state,
  mutations,
  modules: {
    user,
    workspaces,
    table
  }
});

export function useStore(): Store<State> {
  return baseUseStore(key);
}
