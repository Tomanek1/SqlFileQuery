SELECT pub.* 
FROM PublisherBrandEnum pub
WHERE  
	EXISTS (SELECT p.PublisherBrand1EnumID FROM Product p WHERE p.PublisherBrand1EnumID = pub.ID )
	OR
	EXISTS (SELECT p.PublisherBrand2EnumID FROM Product p WHERE p.PublisherBrand2EnumID = pub.ID)
