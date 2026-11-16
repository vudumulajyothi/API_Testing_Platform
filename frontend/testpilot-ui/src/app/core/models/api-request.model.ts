export type HttpMethod = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE' | 'HEAD' | 'OPTIONS';

export interface KeyValuePair {
  key: string;
  value: string;
  isSensitive?: boolean;
}

export interface ExecuteRequest {
  method: HttpMethod;
  url: string;
  headers: KeyValuePair[];
  queryParams: KeyValuePair[];
  body?: string | null;
  requestId?: string;
}

export interface ExecuteResponse {
  statusCode: number;
  statusText: string;
  responseTimeMs: number;
  contentLength: number;
  headers: Record<string, string>;
  body: any;
  rawBody?: string;
  isSuccessStatusCode: boolean;
}

export interface TestCase {
  testCaseId?: string;
  requestId?: string;
  name: string;
  assertionType: 'StatusCode' | 'ResponseTime' | 'JsonFieldEquals' | 'JsonFieldExists' | 'BodyContains';
  expectedValue: string;
  jsonPath?: string;
}
