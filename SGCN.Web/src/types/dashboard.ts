export interface DashboardMetric {
  key: string;
  label: string;
  value: number;
}

export interface DashboardRecentItem {
  type: string;
  title: string;
  description?: string | null;
  reference?: string | null;
  status?: string | null;
  createdAt: string;
}

export interface DashboardChartItem {
  label: string;
  value: number;
}

export type DashboardChartType = 'bar' | 'donut' | 'summary';

export interface DashboardChart {
  key: string;
  title: string;
  type: DashboardChartType;
  items: DashboardChartItem[];
}

export interface Dashboard {
  role: string;
  metrics: DashboardMetric[];
  charts: DashboardChart[];
  recentItemsTitle?: string | null;
  recentItems: DashboardRecentItem[];
}
