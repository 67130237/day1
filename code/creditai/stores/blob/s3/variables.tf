variable "name" {
  description = "Name prefix for resources"
  type        = string
  default     = "creditai-blob"
}

variable "aws_region" {
  description = "AWS region"
  type        = string
  default     = "ap-southeast-1"
}

variable "bucket_name" {
  description = "S3 bucket name"
  type        = string
}

variable "environment" {
  description = "Environment tag"
  type        = string
  default     = "prod"
}

variable "ingest_prefix" {
  description = "Prefix used by ingestion uploads"
  type        = string
  default     = "ingested"
}
