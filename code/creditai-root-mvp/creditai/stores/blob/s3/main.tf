##############################################
# S3 bucket for document storage (template)
# - Private bucket
# - IAM user & policy for write (ingestion) and read (mcp-rag)
# NOTE: Rotate keys and restrict source IPs if possible.
##############################################
terraform {
  required_version = ">= 1.5.0"
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
}

provider "aws" {
  region = var.aws_region
}

resource "aws_s3_bucket" "docs" {
  bucket = var.bucket_name

  tags = {
    Name        = var.bucket_name
    Environment = var.environment
  }
}

resource "aws_s3_bucket_versioning" "docs" {
  bucket = aws_s3_bucket.docs.id
  versioning_configuration { status = "Enabled" }
}

resource "aws_s3_bucket_public_access_block" "docs" {
  bucket                  = aws_s3_bucket.docs.id
  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

# IAM for ingestion (write-only to prefix)
resource "aws_iam_user" "ingester" {
  name = "${var.name}-ingester"
}

data "aws_iam_policy_document" "ingest_policy_doc" {
  statement {
    actions = [
      "s3:PutObject",
      "s3:AbortMultipartUpload",
      "s3:ListBucketMultipartUploads",
      "s3:ListBucket"
    ]
    resources = [
      aws_s3_bucket.docs.arn,
      "${aws_s3_bucket.docs.arn}/${var.ingest_prefix}/*"
    ]
  }
}

resource "aws_iam_policy" "ingest_policy" {
  name   = "${var.name}-ingest-policy"
  policy = data.aws_iam_policy_document.ingest_policy_doc.json
}

resource "aws_iam_user_policy_attachment" "ingester_attach" {
  user       = aws_iam_user.ingester.name
  policy_arn = aws_iam_policy.ingest_policy.arn
}

# IAM for app (read-only from bucket/prefix)
resource "aws_iam_user" "app" {
  name = "${var.name}-app"
}

data "aws_iam_policy_document" "app_policy_doc" {
  statement {
    actions = [
      "s3:GetObject",
      "s3:ListBucket"
    ]
    resources = [
      aws_s3_bucket.docs.arn,
      "${aws_s3_bucket.docs.arn}/*"
    ]
  }
}

resource "aws_iam_policy" "app_policy" {
  name   = "${var.name}-app-policy"
  policy = data.aws_iam_policy_document.app_policy_doc.json
}

resource "aws_iam_user_policy_attachment" "app_attach" {
  user       = aws_iam_user.app.name
  policy_arn = aws_iam_policy.app_policy.arn
}

output "bucket_name" { value = aws_s3_bucket.docs.bucket }
output "ingester_access_key" {
  value     = aws_iam_access_key.ingester.id
  sensitive = true
}
output "ingester_secret_key" {
  value     = aws_iam_access_key.ingester.secret
  sensitive = true
}
output "app_access_key" {
  value     = aws_iam_access_key.app.id
  sensitive = true
}
output "app_secret_key" {
  value     = aws_iam_access_key.app.secret
  sensitive = true
}

# Access keys (consider using roles instead of long-lived keys)
resource "aws_iam_access_key" "ingester" {
  user = aws_iam_user.ingester.name
}

resource "aws_iam_access_key" "app" {
  user = aws_iam_user.app.name
}
