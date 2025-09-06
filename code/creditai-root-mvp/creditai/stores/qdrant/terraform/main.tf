##############################################
# Qdrant on AWS EC2 (template)
# - 1x EC2 instance + security group
# - User data starts Qdrant via Docker
# NOTE: Review & harden before production.
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

data "aws_ami" "amazon_linux" {
  most_recent = true
  owners      = ["amazon"]
  filter {
    name   = "name"
    values = ["al2023-ami-*-x86_64"]
  }
}

resource "aws_security_group" "qdrant_sg" {
  name        = "${var.name}-sg"
  description = "Qdrant security group"
  vpc_id      = var.vpc_id

  ingress {
    description = "Qdrant HTTP"
    from_port   = 6333
    to_port     = 6333
    protocol    = "tcp"
    cidr_blocks = var.allowed_cidrs
  }

  ingress {
    description = "Qdrant gRPC"
    from_port   = 6334
    to_port     = 6334
    protocol    = "tcp"
    cidr_blocks = var.allowed_cidrs
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

resource "aws_instance" "qdrant" {
  ami                         = data.aws_ami.amazon_linux.id
  instance_type               = var.instance_type
  subnet_id                   = var.subnet_id
  vpc_security_group_ids      = [aws_security_group.qdrant_sg.id]
  associate_public_ip_address = var.associate_public_ip

  user_data = <<-EOT
              #!/bin/bash
              set -eux
              yum update -y
              amazon-linux-extras enable docker
              yum install -y docker
              systemctl enable docker && systemctl start docker
              docker pull qdrant/qdrant:latest
              mkdir -p /opt/qdrant/storage
              docker run -d --name qdrant \
                -p 6333:6333 -p 6334:6334 \
                -v /opt/qdrant/storage:/qdrant/storage \
                qdrant/qdrant:latest
              EOT

  tags = {
    Name = "${var.name}"
  }
}

output "qdrant_public_ip" {
  value = aws_instance.qdrant.public_ip
}
