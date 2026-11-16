import { ExecuteRequest } from './api-request.model';

export interface Collection {
  collectionId: string;
  name: string;
  description?: string;
  createdAt: string;
  requestCount: number;
}

export interface SavedApiRequest {
  requestId: string;
  collectionId: string;
  name: string;
  url: string;
  method: string;
  body?: string;
  headers: { key: string; value: string; isSensitive?: boolean }[];
  queryParams: { key: string; value: string }[];
  createdAt: string;
}
