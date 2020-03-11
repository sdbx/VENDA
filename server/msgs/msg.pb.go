// Code generated by protoc-gen-go. DO NOT EDIT.
// versions:
// 	protoc-gen-go v1.20.1
// 	protoc        v3.10.0
// source: msg.proto

package msgs

import (
	proto "github.com/golang/protobuf/proto"
	protoreflect "google.golang.org/protobuf/reflect/protoreflect"
	protoimpl "google.golang.org/protobuf/runtime/protoimpl"
	reflect "reflect"
	sync "sync"
)

const (
	// Verify that this generated code is sufficiently up-to-date.
	_ = protoimpl.EnforceVersion(20 - protoimpl.MinVersion)
	// Verify that runtime/protoimpl is sufficiently up-to-date.
	_ = protoimpl.EnforceVersion(protoimpl.MaxVersion - 20)
)

// This is a compile-time assertion that a sufficiently up-to-date version
// of the legacy proto package is being used.
const _ = proto.ProtoPackageIsVersion4

type CharacterData struct {
	state         protoimpl.MessageState
	sizeCache     protoimpl.SizeCache
	unknownFields protoimpl.UnknownFields

	Id          int32   `protobuf:"varint,1,opt,name=id,proto3" json:"id,omitempty"`
	Name        string  `protobuf:"bytes,2,opt,name=name,proto3" json:"name,omitempty"`
	Hp          float32 `protobuf:"fixed32,3,opt,name=hp,proto3" json:"hp,omitempty"`
	MaxHp       float32 `protobuf:"fixed32,4,opt,name=maxHp,proto3" json:"maxHp,omitempty"`
	X           float32 `protobuf:"fixed32,5,opt,name=x,proto3" json:"x,omitempty"`
	Y           float32 `protobuf:"fixed32,6,opt,name=y,proto3" json:"y,omitempty"`
	FacingRight bool    `protobuf:"varint,7,opt,name=facingRight,proto3" json:"facingRight,omitempty"`
	Speed       float32 `protobuf:"fixed32,8,opt,name=speed,proto3" json:"speed,omitempty"`
	Defense     bool    `protobuf:"varint,9,opt,name=defense,proto3" json:"defense,omitempty"`
	Dead        bool    `protobuf:"varint,10,opt,name=dead,proto3" json:"dead,omitempty"`
}

func (x *CharacterData) Reset() {
	*x = CharacterData{}
	if protoimpl.UnsafeEnabled {
		mi := &file_msg_proto_msgTypes[0]
		ms := protoimpl.X.MessageStateOf(protoimpl.Pointer(x))
		ms.StoreMessageInfo(mi)
	}
}

func (x *CharacterData) String() string {
	return protoimpl.X.MessageStringOf(x)
}

func (*CharacterData) ProtoMessage() {}

func (x *CharacterData) ProtoReflect() protoreflect.Message {
	mi := &file_msg_proto_msgTypes[0]
	if protoimpl.UnsafeEnabled && x != nil {
		ms := protoimpl.X.MessageStateOf(protoimpl.Pointer(x))
		if ms.LoadMessageInfo() == nil {
			ms.StoreMessageInfo(mi)
		}
		return ms
	}
	return mi.MessageOf(x)
}

// Deprecated: Use CharacterData.ProtoReflect.Descriptor instead.
func (*CharacterData) Descriptor() ([]byte, []int) {
	return file_msg_proto_rawDescGZIP(), []int{0}
}

func (x *CharacterData) GetId() int32 {
	if x != nil {
		return x.Id
	}
	return 0
}

func (x *CharacterData) GetName() string {
	if x != nil {
		return x.Name
	}
	return ""
}

func (x *CharacterData) GetHp() float32 {
	if x != nil {
		return x.Hp
	}
	return 0
}

func (x *CharacterData) GetMaxHp() float32 {
	if x != nil {
		return x.MaxHp
	}
	return 0
}

func (x *CharacterData) GetX() float32 {
	if x != nil {
		return x.X
	}
	return 0
}

func (x *CharacterData) GetY() float32 {
	if x != nil {
		return x.Y
	}
	return 0
}

func (x *CharacterData) GetFacingRight() bool {
	if x != nil {
		return x.FacingRight
	}
	return false
}

func (x *CharacterData) GetSpeed() float32 {
	if x != nil {
		return x.Speed
	}
	return 0
}

func (x *CharacterData) GetDefense() bool {
	if x != nil {
		return x.Defense
	}
	return false
}

func (x *CharacterData) GetDead() bool {
	if x != nil {
		return x.Dead
	}
	return false
}

// 1
type InfoEvent struct {
	state         protoimpl.MessageState
	sizeCache     protoimpl.SizeCache
	unknownFields protoimpl.UnknownFields

	Id   int32  `protobuf:"varint,1,opt,name=id,proto3" json:"id,omitempty"`
	Name string `protobuf:"bytes,2,opt,name=name,proto3" json:"name,omitempty"`
}

func (x *InfoEvent) Reset() {
	*x = InfoEvent{}
	if protoimpl.UnsafeEnabled {
		mi := &file_msg_proto_msgTypes[1]
		ms := protoimpl.X.MessageStateOf(protoimpl.Pointer(x))
		ms.StoreMessageInfo(mi)
	}
}

func (x *InfoEvent) String() string {
	return protoimpl.X.MessageStringOf(x)
}

func (*InfoEvent) ProtoMessage() {}

func (x *InfoEvent) ProtoReflect() protoreflect.Message {
	mi := &file_msg_proto_msgTypes[1]
	if protoimpl.UnsafeEnabled && x != nil {
		ms := protoimpl.X.MessageStateOf(protoimpl.Pointer(x))
		if ms.LoadMessageInfo() == nil {
			ms.StoreMessageInfo(mi)
		}
		return ms
	}
	return mi.MessageOf(x)
}

// Deprecated: Use InfoEvent.ProtoReflect.Descriptor instead.
func (*InfoEvent) Descriptor() ([]byte, []int) {
	return file_msg_proto_rawDescGZIP(), []int{1}
}

func (x *InfoEvent) GetId() int32 {
	if x != nil {
		return x.Id
	}
	return 0
}

func (x *InfoEvent) GetName() string {
	if x != nil {
		return x.Name
	}
	return ""
}

// 2
type UserDataEvent struct {
	state         protoimpl.MessageState
	sizeCache     protoimpl.SizeCache
	unknownFields protoimpl.UnknownFields

	Users []*CharacterData `protobuf:"bytes,1,rep,name=users,proto3" json:"users,omitempty"`
}

func (x *UserDataEvent) Reset() {
	*x = UserDataEvent{}
	if protoimpl.UnsafeEnabled {
		mi := &file_msg_proto_msgTypes[2]
		ms := protoimpl.X.MessageStateOf(protoimpl.Pointer(x))
		ms.StoreMessageInfo(mi)
	}
}

func (x *UserDataEvent) String() string {
	return protoimpl.X.MessageStringOf(x)
}

func (*UserDataEvent) ProtoMessage() {}

func (x *UserDataEvent) ProtoReflect() protoreflect.Message {
	mi := &file_msg_proto_msgTypes[2]
	if protoimpl.UnsafeEnabled && x != nil {
		ms := protoimpl.X.MessageStateOf(protoimpl.Pointer(x))
		if ms.LoadMessageInfo() == nil {
			ms.StoreMessageInfo(mi)
		}
		return ms
	}
	return mi.MessageOf(x)
}

// Deprecated: Use UserDataEvent.ProtoReflect.Descriptor instead.
func (*UserDataEvent) Descriptor() ([]byte, []int) {
	return file_msg_proto_rawDescGZIP(), []int{2}
}

func (x *UserDataEvent) GetUsers() []*CharacterData {
	if x != nil {
		return x.Users
	}
	return nil
}

// 3
type MyDataEvent struct {
	state         protoimpl.MessageState
	sizeCache     protoimpl.SizeCache
	unknownFields protoimpl.UnknownFields

	Data *CharacterData `protobuf:"bytes,1,opt,name=data,proto3" json:"data,omitempty"`
}

func (x *MyDataEvent) Reset() {
	*x = MyDataEvent{}
	if protoimpl.UnsafeEnabled {
		mi := &file_msg_proto_msgTypes[3]
		ms := protoimpl.X.MessageStateOf(protoimpl.Pointer(x))
		ms.StoreMessageInfo(mi)
	}
}

func (x *MyDataEvent) String() string {
	return protoimpl.X.MessageStringOf(x)
}

func (*MyDataEvent) ProtoMessage() {}

func (x *MyDataEvent) ProtoReflect() protoreflect.Message {
	mi := &file_msg_proto_msgTypes[3]
	if protoimpl.UnsafeEnabled && x != nil {
		ms := protoimpl.X.MessageStateOf(protoimpl.Pointer(x))
		if ms.LoadMessageInfo() == nil {
			ms.StoreMessageInfo(mi)
		}
		return ms
	}
	return mi.MessageOf(x)
}

// Deprecated: Use MyDataEvent.ProtoReflect.Descriptor instead.
func (*MyDataEvent) Descriptor() ([]byte, []int) {
	return file_msg_proto_rawDescGZIP(), []int{3}
}

func (x *MyDataEvent) GetData() *CharacterData {
	if x != nil {
		return x.Data
	}
	return nil
}

// 4
type HitEvent struct {
	state         protoimpl.MessageState
	sizeCache     protoimpl.SizeCache
	unknownFields protoimpl.UnknownFields

	Id     int32   `protobuf:"varint,1,opt,name=id,proto3" json:"id,omitempty"`
	Target int32   `protobuf:"varint,2,opt,name=target,proto3" json:"target,omitempty"`
	Dmg    float32 `protobuf:"fixed32,3,opt,name=dmg,proto3" json:"dmg,omitempty"`
}

func (x *HitEvent) Reset() {
	*x = HitEvent{}
	if protoimpl.UnsafeEnabled {
		mi := &file_msg_proto_msgTypes[4]
		ms := protoimpl.X.MessageStateOf(protoimpl.Pointer(x))
		ms.StoreMessageInfo(mi)
	}
}

func (x *HitEvent) String() string {
	return protoimpl.X.MessageStringOf(x)
}

func (*HitEvent) ProtoMessage() {}

func (x *HitEvent) ProtoReflect() protoreflect.Message {
	mi := &file_msg_proto_msgTypes[4]
	if protoimpl.UnsafeEnabled && x != nil {
		ms := protoimpl.X.MessageStateOf(protoimpl.Pointer(x))
		if ms.LoadMessageInfo() == nil {
			ms.StoreMessageInfo(mi)
		}
		return ms
	}
	return mi.MessageOf(x)
}

// Deprecated: Use HitEvent.ProtoReflect.Descriptor instead.
func (*HitEvent) Descriptor() ([]byte, []int) {
	return file_msg_proto_rawDescGZIP(), []int{4}
}

func (x *HitEvent) GetId() int32 {
	if x != nil {
		return x.Id
	}
	return 0
}

func (x *HitEvent) GetTarget() int32 {
	if x != nil {
		return x.Target
	}
	return 0
}

func (x *HitEvent) GetDmg() float32 {
	if x != nil {
		return x.Dmg
	}
	return 0
}

// 5
type DeathEvent struct {
	state         protoimpl.MessageState
	sizeCache     protoimpl.SizeCache
	unknownFields protoimpl.UnknownFields

	By int32 `protobuf:"varint,1,opt,name=by,proto3" json:"by,omitempty"`
	Id int32 `protobuf:"varint,2,opt,name=id,proto3" json:"id,omitempty"`
}

func (x *DeathEvent) Reset() {
	*x = DeathEvent{}
	if protoimpl.UnsafeEnabled {
		mi := &file_msg_proto_msgTypes[5]
		ms := protoimpl.X.MessageStateOf(protoimpl.Pointer(x))
		ms.StoreMessageInfo(mi)
	}
}

func (x *DeathEvent) String() string {
	return protoimpl.X.MessageStringOf(x)
}

func (*DeathEvent) ProtoMessage() {}

func (x *DeathEvent) ProtoReflect() protoreflect.Message {
	mi := &file_msg_proto_msgTypes[5]
	if protoimpl.UnsafeEnabled && x != nil {
		ms := protoimpl.X.MessageStateOf(protoimpl.Pointer(x))
		if ms.LoadMessageInfo() == nil {
			ms.StoreMessageInfo(mi)
		}
		return ms
	}
	return mi.MessageOf(x)
}

// Deprecated: Use DeathEvent.ProtoReflect.Descriptor instead.
func (*DeathEvent) Descriptor() ([]byte, []int) {
	return file_msg_proto_rawDescGZIP(), []int{5}
}

func (x *DeathEvent) GetBy() int32 {
	if x != nil {
		return x.By
	}
	return 0
}

func (x *DeathEvent) GetId() int32 {
	if x != nil {
		return x.Id
	}
	return 0
}

// 6
type AnimateEvent struct {
	state         protoimpl.MessageState
	sizeCache     protoimpl.SizeCache
	unknownFields protoimpl.UnknownFields

	Id    int32 `protobuf:"varint,1,opt,name=id,proto3" json:"id,omitempty"`
	Anime int32 `protobuf:"varint,2,opt,name=anime,proto3" json:"anime,omitempty"`
}

func (x *AnimateEvent) Reset() {
	*x = AnimateEvent{}
	if protoimpl.UnsafeEnabled {
		mi := &file_msg_proto_msgTypes[6]
		ms := protoimpl.X.MessageStateOf(protoimpl.Pointer(x))
		ms.StoreMessageInfo(mi)
	}
}

func (x *AnimateEvent) String() string {
	return protoimpl.X.MessageStringOf(x)
}

func (*AnimateEvent) ProtoMessage() {}

func (x *AnimateEvent) ProtoReflect() protoreflect.Message {
	mi := &file_msg_proto_msgTypes[6]
	if protoimpl.UnsafeEnabled && x != nil {
		ms := protoimpl.X.MessageStateOf(protoimpl.Pointer(x))
		if ms.LoadMessageInfo() == nil {
			ms.StoreMessageInfo(mi)
		}
		return ms
	}
	return mi.MessageOf(x)
}

// Deprecated: Use AnimateEvent.ProtoReflect.Descriptor instead.
func (*AnimateEvent) Descriptor() ([]byte, []int) {
	return file_msg_proto_rawDescGZIP(), []int{6}
}

func (x *AnimateEvent) GetId() int32 {
	if x != nil {
		return x.Id
	}
	return 0
}

func (x *AnimateEvent) GetAnime() int32 {
	if x != nil {
		return x.Anime
	}
	return 0
}

// 7
type DisconnectedEvent struct {
	state         protoimpl.MessageState
	sizeCache     protoimpl.SizeCache
	unknownFields protoimpl.UnknownFields

	Id int32 `protobuf:"varint,1,opt,name=id,proto3" json:"id,omitempty"`
}

func (x *DisconnectedEvent) Reset() {
	*x = DisconnectedEvent{}
	if protoimpl.UnsafeEnabled {
		mi := &file_msg_proto_msgTypes[7]
		ms := protoimpl.X.MessageStateOf(protoimpl.Pointer(x))
		ms.StoreMessageInfo(mi)
	}
}

func (x *DisconnectedEvent) String() string {
	return protoimpl.X.MessageStringOf(x)
}

func (*DisconnectedEvent) ProtoMessage() {}

func (x *DisconnectedEvent) ProtoReflect() protoreflect.Message {
	mi := &file_msg_proto_msgTypes[7]
	if protoimpl.UnsafeEnabled && x != nil {
		ms := protoimpl.X.MessageStateOf(protoimpl.Pointer(x))
		if ms.LoadMessageInfo() == nil {
			ms.StoreMessageInfo(mi)
		}
		return ms
	}
	return mi.MessageOf(x)
}

// Deprecated: Use DisconnectedEvent.ProtoReflect.Descriptor instead.
func (*DisconnectedEvent) Descriptor() ([]byte, []int) {
	return file_msg_proto_rawDescGZIP(), []int{7}
}

func (x *DisconnectedEvent) GetId() int32 {
	if x != nil {
		return x.Id
	}
	return 0
}

var File_msg_proto protoreflect.FileDescriptor

var file_msg_proto_rawDesc = []byte{
	0x0a, 0x09, 0x6d, 0x73, 0x67, 0x2e, 0x70, 0x72, 0x6f, 0x74, 0x6f, 0x22, 0xdb, 0x01, 0x0a, 0x0d,
	0x43, 0x68, 0x61, 0x72, 0x61, 0x63, 0x74, 0x65, 0x72, 0x44, 0x61, 0x74, 0x61, 0x12, 0x0e, 0x0a,
	0x02, 0x69, 0x64, 0x18, 0x01, 0x20, 0x01, 0x28, 0x05, 0x52, 0x02, 0x69, 0x64, 0x12, 0x12, 0x0a,
	0x04, 0x6e, 0x61, 0x6d, 0x65, 0x18, 0x02, 0x20, 0x01, 0x28, 0x09, 0x52, 0x04, 0x6e, 0x61, 0x6d,
	0x65, 0x12, 0x0e, 0x0a, 0x02, 0x68, 0x70, 0x18, 0x03, 0x20, 0x01, 0x28, 0x02, 0x52, 0x02, 0x68,
	0x70, 0x12, 0x14, 0x0a, 0x05, 0x6d, 0x61, 0x78, 0x48, 0x70, 0x18, 0x04, 0x20, 0x01, 0x28, 0x02,
	0x52, 0x05, 0x6d, 0x61, 0x78, 0x48, 0x70, 0x12, 0x0c, 0x0a, 0x01, 0x78, 0x18, 0x05, 0x20, 0x01,
	0x28, 0x02, 0x52, 0x01, 0x78, 0x12, 0x0c, 0x0a, 0x01, 0x79, 0x18, 0x06, 0x20, 0x01, 0x28, 0x02,
	0x52, 0x01, 0x79, 0x12, 0x20, 0x0a, 0x0b, 0x66, 0x61, 0x63, 0x69, 0x6e, 0x67, 0x52, 0x69, 0x67,
	0x68, 0x74, 0x18, 0x07, 0x20, 0x01, 0x28, 0x08, 0x52, 0x0b, 0x66, 0x61, 0x63, 0x69, 0x6e, 0x67,
	0x52, 0x69, 0x67, 0x68, 0x74, 0x12, 0x14, 0x0a, 0x05, 0x73, 0x70, 0x65, 0x65, 0x64, 0x18, 0x08,
	0x20, 0x01, 0x28, 0x02, 0x52, 0x05, 0x73, 0x70, 0x65, 0x65, 0x64, 0x12, 0x18, 0x0a, 0x07, 0x64,
	0x65, 0x66, 0x65, 0x6e, 0x73, 0x65, 0x18, 0x09, 0x20, 0x01, 0x28, 0x08, 0x52, 0x07, 0x64, 0x65,
	0x66, 0x65, 0x6e, 0x73, 0x65, 0x12, 0x12, 0x0a, 0x04, 0x64, 0x65, 0x61, 0x64, 0x18, 0x0a, 0x20,
	0x01, 0x28, 0x08, 0x52, 0x04, 0x64, 0x65, 0x61, 0x64, 0x22, 0x2f, 0x0a, 0x09, 0x49, 0x6e, 0x66,
	0x6f, 0x45, 0x76, 0x65, 0x6e, 0x74, 0x12, 0x0e, 0x0a, 0x02, 0x69, 0x64, 0x18, 0x01, 0x20, 0x01,
	0x28, 0x05, 0x52, 0x02, 0x69, 0x64, 0x12, 0x12, 0x0a, 0x04, 0x6e, 0x61, 0x6d, 0x65, 0x18, 0x02,
	0x20, 0x01, 0x28, 0x09, 0x52, 0x04, 0x6e, 0x61, 0x6d, 0x65, 0x22, 0x35, 0x0a, 0x0d, 0x55, 0x73,
	0x65, 0x72, 0x44, 0x61, 0x74, 0x61, 0x45, 0x76, 0x65, 0x6e, 0x74, 0x12, 0x24, 0x0a, 0x05, 0x75,
	0x73, 0x65, 0x72, 0x73, 0x18, 0x01, 0x20, 0x03, 0x28, 0x0b, 0x32, 0x0e, 0x2e, 0x43, 0x68, 0x61,
	0x72, 0x61, 0x63, 0x74, 0x65, 0x72, 0x44, 0x61, 0x74, 0x61, 0x52, 0x05, 0x75, 0x73, 0x65, 0x72,
	0x73, 0x22, 0x31, 0x0a, 0x0b, 0x4d, 0x79, 0x44, 0x61, 0x74, 0x61, 0x45, 0x76, 0x65, 0x6e, 0x74,
	0x12, 0x22, 0x0a, 0x04, 0x64, 0x61, 0x74, 0x61, 0x18, 0x01, 0x20, 0x01, 0x28, 0x0b, 0x32, 0x0e,
	0x2e, 0x43, 0x68, 0x61, 0x72, 0x61, 0x63, 0x74, 0x65, 0x72, 0x44, 0x61, 0x74, 0x61, 0x52, 0x04,
	0x64, 0x61, 0x74, 0x61, 0x22, 0x44, 0x0a, 0x08, 0x48, 0x69, 0x74, 0x45, 0x76, 0x65, 0x6e, 0x74,
	0x12, 0x0e, 0x0a, 0x02, 0x69, 0x64, 0x18, 0x01, 0x20, 0x01, 0x28, 0x05, 0x52, 0x02, 0x69, 0x64,
	0x12, 0x16, 0x0a, 0x06, 0x74, 0x61, 0x72, 0x67, 0x65, 0x74, 0x18, 0x02, 0x20, 0x01, 0x28, 0x05,
	0x52, 0x06, 0x74, 0x61, 0x72, 0x67, 0x65, 0x74, 0x12, 0x10, 0x0a, 0x03, 0x64, 0x6d, 0x67, 0x18,
	0x03, 0x20, 0x01, 0x28, 0x02, 0x52, 0x03, 0x64, 0x6d, 0x67, 0x22, 0x2c, 0x0a, 0x0a, 0x44, 0x65,
	0x61, 0x74, 0x68, 0x45, 0x76, 0x65, 0x6e, 0x74, 0x12, 0x0e, 0x0a, 0x02, 0x62, 0x79, 0x18, 0x01,
	0x20, 0x01, 0x28, 0x05, 0x52, 0x02, 0x62, 0x79, 0x12, 0x0e, 0x0a, 0x02, 0x69, 0x64, 0x18, 0x02,
	0x20, 0x01, 0x28, 0x05, 0x52, 0x02, 0x69, 0x64, 0x22, 0x34, 0x0a, 0x0c, 0x41, 0x6e, 0x69, 0x6d,
	0x61, 0x74, 0x65, 0x45, 0x76, 0x65, 0x6e, 0x74, 0x12, 0x0e, 0x0a, 0x02, 0x69, 0x64, 0x18, 0x01,
	0x20, 0x01, 0x28, 0x05, 0x52, 0x02, 0x69, 0x64, 0x12, 0x14, 0x0a, 0x05, 0x61, 0x6e, 0x69, 0x6d,
	0x65, 0x18, 0x02, 0x20, 0x01, 0x28, 0x05, 0x52, 0x05, 0x61, 0x6e, 0x69, 0x6d, 0x65, 0x22, 0x23,
	0x0a, 0x11, 0x44, 0x69, 0x73, 0x63, 0x6f, 0x6e, 0x6e, 0x65, 0x63, 0x74, 0x65, 0x64, 0x45, 0x76,
	0x65, 0x6e, 0x74, 0x12, 0x0e, 0x0a, 0x02, 0x69, 0x64, 0x18, 0x01, 0x20, 0x01, 0x28, 0x05, 0x52,
	0x02, 0x69, 0x64, 0x42, 0x0d, 0x5a, 0x0b, 0x73, 0x65, 0x72, 0x76, 0x65, 0x72, 0x2f, 0x6d, 0x73,
	0x67, 0x73, 0x62, 0x06, 0x70, 0x72, 0x6f, 0x74, 0x6f, 0x33,
}

var (
	file_msg_proto_rawDescOnce sync.Once
	file_msg_proto_rawDescData = file_msg_proto_rawDesc
)

func file_msg_proto_rawDescGZIP() []byte {
	file_msg_proto_rawDescOnce.Do(func() {
		file_msg_proto_rawDescData = protoimpl.X.CompressGZIP(file_msg_proto_rawDescData)
	})
	return file_msg_proto_rawDescData
}

var file_msg_proto_msgTypes = make([]protoimpl.MessageInfo, 8)
var file_msg_proto_goTypes = []interface{}{
	(*CharacterData)(nil),     // 0: CharacterData
	(*InfoEvent)(nil),         // 1: InfoEvent
	(*UserDataEvent)(nil),     // 2: UserDataEvent
	(*MyDataEvent)(nil),       // 3: MyDataEvent
	(*HitEvent)(nil),          // 4: HitEvent
	(*DeathEvent)(nil),        // 5: DeathEvent
	(*AnimateEvent)(nil),      // 6: AnimateEvent
	(*DisconnectedEvent)(nil), // 7: DisconnectedEvent
}
var file_msg_proto_depIdxs = []int32{
	0, // 0: UserDataEvent.users:type_name -> CharacterData
	0, // 1: MyDataEvent.data:type_name -> CharacterData
	2, // [2:2] is the sub-list for method output_type
	2, // [2:2] is the sub-list for method input_type
	2, // [2:2] is the sub-list for extension type_name
	2, // [2:2] is the sub-list for extension extendee
	0, // [0:2] is the sub-list for field type_name
}

func init() { file_msg_proto_init() }
func file_msg_proto_init() {
	if File_msg_proto != nil {
		return
	}
	if !protoimpl.UnsafeEnabled {
		file_msg_proto_msgTypes[0].Exporter = func(v interface{}, i int) interface{} {
			switch v := v.(*CharacterData); i {
			case 0:
				return &v.state
			case 1:
				return &v.sizeCache
			case 2:
				return &v.unknownFields
			default:
				return nil
			}
		}
		file_msg_proto_msgTypes[1].Exporter = func(v interface{}, i int) interface{} {
			switch v := v.(*InfoEvent); i {
			case 0:
				return &v.state
			case 1:
				return &v.sizeCache
			case 2:
				return &v.unknownFields
			default:
				return nil
			}
		}
		file_msg_proto_msgTypes[2].Exporter = func(v interface{}, i int) interface{} {
			switch v := v.(*UserDataEvent); i {
			case 0:
				return &v.state
			case 1:
				return &v.sizeCache
			case 2:
				return &v.unknownFields
			default:
				return nil
			}
		}
		file_msg_proto_msgTypes[3].Exporter = func(v interface{}, i int) interface{} {
			switch v := v.(*MyDataEvent); i {
			case 0:
				return &v.state
			case 1:
				return &v.sizeCache
			case 2:
				return &v.unknownFields
			default:
				return nil
			}
		}
		file_msg_proto_msgTypes[4].Exporter = func(v interface{}, i int) interface{} {
			switch v := v.(*HitEvent); i {
			case 0:
				return &v.state
			case 1:
				return &v.sizeCache
			case 2:
				return &v.unknownFields
			default:
				return nil
			}
		}
		file_msg_proto_msgTypes[5].Exporter = func(v interface{}, i int) interface{} {
			switch v := v.(*DeathEvent); i {
			case 0:
				return &v.state
			case 1:
				return &v.sizeCache
			case 2:
				return &v.unknownFields
			default:
				return nil
			}
		}
		file_msg_proto_msgTypes[6].Exporter = func(v interface{}, i int) interface{} {
			switch v := v.(*AnimateEvent); i {
			case 0:
				return &v.state
			case 1:
				return &v.sizeCache
			case 2:
				return &v.unknownFields
			default:
				return nil
			}
		}
		file_msg_proto_msgTypes[7].Exporter = func(v interface{}, i int) interface{} {
			switch v := v.(*DisconnectedEvent); i {
			case 0:
				return &v.state
			case 1:
				return &v.sizeCache
			case 2:
				return &v.unknownFields
			default:
				return nil
			}
		}
	}
	type x struct{}
	out := protoimpl.TypeBuilder{
		File: protoimpl.DescBuilder{
			GoPackagePath: reflect.TypeOf(x{}).PkgPath(),
			RawDescriptor: file_msg_proto_rawDesc,
			NumEnums:      0,
			NumMessages:   8,
			NumExtensions: 0,
			NumServices:   0,
		},
		GoTypes:           file_msg_proto_goTypes,
		DependencyIndexes: file_msg_proto_depIdxs,
		MessageInfos:      file_msg_proto_msgTypes,
	}.Build()
	File_msg_proto = out.File
	file_msg_proto_rawDesc = nil
	file_msg_proto_goTypes = nil
	file_msg_proto_depIdxs = nil
}
