import { Module } from "vuex";
import { state } from "@/store/modules/user/state";
import { getters } from "@/store/modules/user/getters";
import { mutations } from "@/store/modules/user/mutations";
import { actions } from "@/store/modules/user/actions";
import { State, UserState } from "@/models/store";

export default {
  namespaced: true,
  state,
  getters,
  mutations,
  actions
} as Module<UserState, State>;
