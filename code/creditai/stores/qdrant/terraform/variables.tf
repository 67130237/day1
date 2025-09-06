variable "name" {
  description = "Name prefix for resources"
  type        = string
  default     = "creditai-qdrant"
}

variable "aws_region" {
  description = "AWS region"
  type        = string
  default     = "ap-southeast-1"
}

variable "vpc_id" {
  description = "VPC ID"
  type        = string
}

variable "subnet_id" {
  description = "Subnet ID"
  type        = string
}

variable "allowed_cidrs" {
  description = "CIDRs allowed to access Qdrant ports"
  type        = list(string)
  default     = ["0.0.0.0/0"]
}

variable "instance_type" {
  description = "EC2 instance type"
  type        = string
  default     = "t3.medium"
}

variable "associate_public_ip" {
  description = "Associate public IP"
  type        = bool
  default     = true
}
