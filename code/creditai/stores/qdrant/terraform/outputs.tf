output "qdrant_sg_id" {
  value = aws_security_group.qdrant_sg.id
}

output "qdrant_instance_id" {
  value = aws_instance.qdrant.id
}
