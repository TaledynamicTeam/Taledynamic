import BaseResponse from "@/interfaces/api/responses/base";
import { User } from "@/interfaces/api/responses/user";

interface Workspace {
	id: number,
	name: string,
	created: string,
	modified: string,
	user: User
}

export interface CreateWorkspaceResponse extends BaseResponse {}

export interface DeleteWorkspaceResponse extends BaseResponse {}

export interface UpdateWorkspaceResponse extends BaseResponse {}

export interface GetWorkspaceByIdResponse extends BaseResponse {
	workspace: Workspace
}

export interface GetWorkspacesByUserResponse extends BaseResponse {
	workspaces: Workspace[]
}